import { create } from "zustand";
import { deleteApiProfileClaim, getApiProfile, postApiProfileClaim } from "@/api/client";
import type { ProfileModel } from "@/api/client";
import { LoadingStates } from "@/lib/loadingStates";

interface ProfileState {
    profile: ProfileModel | null;
    state: LoadingStates;
    claimError: string | null;
}

interface ProfileActions {
    fetchProfile: () => Promise<void>;
    /** Claims a pilot name; resolves to true when the claim was accepted. */
    claimPilot: (pilotName: string) => Promise<boolean>;
    cancelClaim: () => Promise<void>;
    clearClaimError: () => void;
}

type ProfileStore = ProfileState & ProfileActions;

export const useProfileStore = create<ProfileStore>()((set, get) => ({
    profile: null,
    state: "Idle",
    claimError: null,

    fetchProfile: async () => {
        if (get().state === "Loading") return;

        set({ state: "Loading" });
        try {
            const response = await getApiProfile();
            set(response.data ? { profile: response.data, state: "Loaded" } : { state: "Error" });
        } catch {
            set({ state: "Error" });
        }
    },

    claimPilot: async (pilotName: string) => {
        set({ claimError: null });
        try {
            const response = await postApiProfileClaim({ body: { pilotName } });
            if (response.data) {
                set({ profile: response.data });
                return true;
            }

            set({
                claimError:
                    response.response?.status === 409
                        ? "This pilot is already linked or being claimed by another account."
                        : "Could not claim this pilot. Please try again.",
            });
            return false;
        } catch {
            set({ claimError: "Network error. Please try again." });
            return false;
        }
    },

    cancelClaim: async () => {
        try {
            const response = await deleteApiProfileClaim();
            if (response.data) {
                set({ profile: response.data });
            }
        } catch {
            // The claim stays visible; the user can retry
        }
    },

    clearClaimError: () => set({ claimError: null }),
}));
