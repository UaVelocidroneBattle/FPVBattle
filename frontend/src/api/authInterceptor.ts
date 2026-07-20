import { client } from "@/api/client";
import { useAuthStore } from "@/store/authStore";

// Endpoints that authenticate by their payload; attaching (or refreshing) an
// access token here is pointless and would deadlock the single-flight refresh.
const anonymousAuthPaths = ["/api/auth/Google", "/api/auth/Refresh", "/api/auth/Logout"];

/**
 * Attaches a valid Bearer token to outgoing API requests, transparently
 * refreshing it just before expiry. No-op while signed out.
 */
export function registerAuthInterceptor() {
    client.interceptors.request.use(async (request) => {
        if (anonymousAuthPaths.some((path) => request.url.includes(path))) return request;

        const token = await useAuthStore.getState().getValidAccessToken();
        if (token) {
            request.headers.set("Authorization", `Bearer ${token}`);
        }
        return request;
    });
}
