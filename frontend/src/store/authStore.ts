import { create } from "zustand";
import {
    getApiAuthMe,
    postApiAuthGoogle,
    postApiAuthLogout,
    postApiAuthRefresh,
} from "@/api/client";
import type { AuthTokensModel, CurrentUserModel } from "@/api/client";
import { LoadingStates } from "@/lib/loadingStates";

const REFRESH_TOKEN_KEY = "auth.refreshToken";

/** Refresh slightly before the access token actually expires. */
const EXPIRY_MARGIN_MS = 30_000;

export interface AuthState {
    user: CurrentUserModel | null;
    accessToken: string | null;
    accessTokenExpiresAt: number | null;
    state: LoadingStates;
}

export interface AuthActions {
    loginWithGoogle: (idToken: string) => Promise<void>;
    logout: () => Promise<void>;
    /** Restores the session from the stored refresh token on app start. */
    restore: () => Promise<void>;
    /** Returns a non-expired access token, refreshing when needed; null when signed out. */
    getValidAccessToken: () => Promise<string | null>;
}

export type AuthStore = AuthState & AuthActions;

export const useAuthStore = create<AuthStore>()((set, get) => {
    const applyTokens = (tokens: AuthTokensModel) => {
        localStorage.setItem(REFRESH_TOKEN_KEY, tokens.refreshToken);
        set({
            accessToken: tokens.accessToken,
            accessTokenExpiresAt: Date.parse(tokens.accessTokenExpiresAt),
        });
    };

    const clearSession = () => {
        localStorage.removeItem(REFRESH_TOKEN_KEY);
        set({ user: null, accessToken: null, accessTokenExpiresAt: null });
    };

    // Refresh tokens are rotated server-side and valid only once, so concurrent
    // callers must share a single in-flight refresh.
    let refreshPromise: Promise<boolean> | null = null;

    const refresh = (): Promise<boolean> => {
        refreshPromise ??= (async () => {
            const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
            if (!refreshToken) return false;

            try {
                const response = await postApiAuthRefresh({ body: { refreshToken } });
                if (!response.data) {
                    // The token was rejected — the session is over
                    clearSession();
                    return false;
                }
                applyTokens(response.data);
                return true;
            } catch {
                // Network error — keep the session and let a later call retry
                return false;
            } finally {
                refreshPromise = null;
            }
        })();

        return refreshPromise;
    };

    return {
        user: null,
        accessToken: null,
        accessTokenExpiresAt: null,
        state: "Idle",

        loginWithGoogle: async (idToken: string) => {
            set({ state: "Loading" });
            try {
                const response = await postApiAuthGoogle({ body: { idToken } });
                if (!response.data) throw new Error("Sign-in was rejected");
                applyTokens(response.data);

                const me = await getApiAuthMe();
                if (!me.data) throw new Error("Failed to load the signed-in user");
                set({ user: me.data, state: "Loaded" });
            } catch {
                clearSession();
                set({ state: "Error" });
            }
        },

        logout: async () => {
            const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
            if (refreshToken) {
                try {
                    await postApiAuthLogout({ body: { refreshToken } });
                } catch {
                    // Best effort — the local session is cleared regardless
                }
            }
            clearSession();
            set({ state: "Idle" });
        },

        restore: async () => {
            if (!localStorage.getItem(REFRESH_TOKEN_KEY)) return;

            set({ state: "Loading" });
            if (!(await refresh())) {
                set({ state: "Idle" });
                return;
            }

            const me = await getApiAuthMe();
            set(me.data ? { user: me.data, state: "Loaded" } : { state: "Idle" });
        },

        getValidAccessToken: async () => {
            const { accessToken, accessTokenExpiresAt } = get();
            const stillValid =
                accessToken !== null &&
                accessTokenExpiresAt !== null &&
                Date.now() < accessTokenExpiresAt - EXPIRY_MARGIN_MS;

            if (stillValid) return accessToken;
            if (!localStorage.getItem(REFRESH_TOKEN_KEY)) return null;

            return (await refresh()) ? get().accessToken : null;
        },
    };
});
