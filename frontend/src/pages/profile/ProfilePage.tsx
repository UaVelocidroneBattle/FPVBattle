import { useEffect } from "react";
import { useAuthStore } from "@/store/authStore";
import { useProfileStore } from "@/store/profileStore";
import PilotBindingSection from "./PilotBindingSection";

/**
 * The signed-in user's own profile: account details from Google
 * and the pilot linked to the account.
 */
function ProfilePage() {
    const user = useAuthStore((state) => state.user);
    const authState = useAuthStore((state) => state.state);
    const profile = useProfileStore((state) => state.profile);
    const profileState = useProfileStore((state) => state.state);

    useEffect(() => {
        if (!user) return;
        useProfileStore.getState().fetchProfile();
    }, [user]);

    if (!user) {
        return (
            <p className="mt-8 text-center text-slate-400">
                {authState === "Loading" ? "Loading…" : "Sign in to view your profile."}
            </p>
        );
    }

    return (
        <div className="mx-auto w-full max-w-2xl">
            <h1 className="mb-6 text-2xl font-bold">Profile</h1>

            <section className="rounded-lg border border-slate-700 bg-slate-900/60 p-5">
                <h2 className="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-400">
                    Account
                </h2>
                <p className="text-lg font-semibold">{profile?.displayName ?? user.displayName}</p>
                <p className="text-sm text-slate-400">{profile?.email ?? user.email}</p>
            </section>

            <section className="mt-6 rounded-lg border border-slate-700 bg-slate-900/60 p-5">
                <h2 className="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-400">
                    My Velocidrone pilot
                </h2>
                {profile ? (
                    <PilotBindingSection profile={profile} />
                ) : (
                    <p className="text-sm text-slate-400">
                        {profileState === "Error" ? "Failed to load the profile." : "Loading…"}
                    </p>
                )}
            </section>
        </div>
    );
}

export default ProfilePage;
