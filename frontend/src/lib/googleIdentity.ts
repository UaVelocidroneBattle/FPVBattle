/**
 * Loads the Google Identity Services script on demand and exposes the minimal
 * typed surface we use (ID-token sign-in button).
 * https://developers.google.com/identity/gsi/web
 */

export interface GoogleCredentialResponse {
    credential: string;
}

interface GoogleIdentityApi {
    accounts: {
        id: {
            initialize: (config: {
                client_id: string;
                callback: (response: GoogleCredentialResponse) => void;
            }) => void;
            renderButton: (
                parent: HTMLElement,
                options: {
                    theme?: "outline" | "filled_black" | "filled_blue";
                    size?: "large" | "medium" | "small";
                    shape?: "rectangular" | "pill" | "circle" | "square";
                    text?: "signin_with" | "signup_with" | "continue_with" | "signin";
                    width?: number;
                },
            ) => void;
        };
    };
}

declare global {
    interface Window {
        google?: GoogleIdentityApi;
    }
}

let loader: Promise<GoogleIdentityApi> | null = null;

export function loadGoogleIdentity(): Promise<GoogleIdentityApi> {
    loader ??= new Promise((resolve, reject) => {
        if (window.google) {
            resolve(window.google);
            return;
        }

        const script = document.createElement("script");
        script.src = "https://accounts.google.com/gsi/client";
        script.async = true;
        script.onload = () =>
            window.google
                ? resolve(window.google)
                : reject(new Error("Google Identity Services script loaded without the google global"));
        script.onerror = () => reject(new Error("Failed to load Google Identity Services"));
        document.head.appendChild(script);
    });

    return loader;
}
