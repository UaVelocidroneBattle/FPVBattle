import { useAuthStore } from "@/store/authStore";
import { useProfileStore } from "@/store/profileStore";

/**
 * Name of the pilot to highlight in leaderboards: the one linked to the signed
 * in user, or null when nobody is signed in or no pilot is linked.
 */
export function useHighlightedPilot(): string | null {
    const user = useAuthStore((state) => state.user);
    const linkedPilotName = useProfileStore((state) => state.profile?.pilot?.name);

    return user && linkedPilotName ? linkedPilotName : null;
}
