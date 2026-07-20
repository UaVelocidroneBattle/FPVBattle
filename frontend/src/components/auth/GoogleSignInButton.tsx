import { useEffect, useRef, useState } from "react";
import { loadGoogleIdentity } from "@/lib/googleIdentity";
import { useAuthStore } from "@/store/authStore";

const clientId = import.meta.env.VITE_GOOGLE_CLIENT_ID;

/** Shared by the styled visual and the invisible Google button so their click areas align. */
const BUTTON_WIDTH_PX = 120;

/** Monochrome Google "G" that inherits the surrounding text color, like the other site icons. */
function GoogleLogo() {
    return (
        <svg className="h-4 w-4" viewBox="0 0 48 48" fill="currentColor" aria-hidden="true">
            <path d="M24 9.5c3.54 0 6.71 1.22 9.21 3.6l6.85-6.85C35.9 2.38 30.47 0 24 0 14.62 0 6.51 5.38 2.56 13.22l7.98 6.19C12.43 13.72 17.74 9.5 24 9.5z" />
            <path d="M46.98 24.55c0-1.57-.15-3.09-.38-4.55H24v9.02h12.94c-.58 2.96-2.26 5.48-4.78 7.18l7.73 6c4.51-4.18 7.09-10.36 7.09-17.65z" />
            <path d="M10.53 28.59c-.48-1.45-.76-2.99-.76-4.59s.27-3.14.76-4.59l-7.98-6.19C.92 16.46 0 20.12 0 24c0 3.88.92 7.54 2.56 10.78l7.97-6.19z" />
            <path d="M24 48c6.48 0 11.93-2.13 15.89-5.81l-7.73-6c-2.15 1.45-4.92 2.3-8.16 2.3-6.26 0-11.57-4.22-13.47-9.91l-7.98 6.19C6.51 42.62 14.62 48 24 48z" />
        </svg>
    );
}

/**
 * Google sign-in styled to match the site. Google's iframe button cannot be
 * themed, so the real button is rendered invisible on top of a site-styled
 * visual: Google still handles the click and issues the ID token, which is
 * exchanged for app tokens via the auth store.
 */
function GoogleSignInButton() {
    const googleButtonRef = useRef<HTMLDivElement>(null);
    const [failed, setFailed] = useState(false);

    useEffect(() => {
        if (!clientId) return;

        let cancelled = false;
        loadGoogleIdentity()
            .then((google) => {
                if (cancelled || !googleButtonRef.current) return;

                google.accounts.id.initialize({
                    client_id: clientId,
                    callback: (response) => {
                        const { loginWithGoogle } = useAuthStore.getState();
                        loginWithGoogle(response.credential);
                    },
                });
                google.accounts.id.renderButton(googleButtonRef.current, {
                    size: "medium",
                    text: "signin",
                    width: BUTTON_WIDTH_PX,
                });
            })
            .catch(() => {
                if (!cancelled) setFailed(true);
            });

        return () => {
            cancelled = true;
        };
    }, []);

    if (!clientId || failed) return null;

    return (
        <div className="group relative h-8" style={{ width: BUTTON_WIDTH_PX }}>
            <div className="flex h-full w-full items-center justify-center gap-2 rounded-full border border-slate-600 text-sm text-slate-300 transition-colors group-hover:border-emerald-400 group-hover:text-emerald-400">
                <GoogleLogo />
                Sign in
            </div>
            {/* The clickable Google button, sized to cover the visual above */}
            <div ref={googleButtonRef} className="absolute inset-0 overflow-hidden opacity-0" />
        </div>
    );
}

export default GoogleSignInButton;
