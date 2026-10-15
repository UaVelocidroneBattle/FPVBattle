import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import type { TeamsOverviewModel } from "@/api/client";
import { Spinner } from "@/components/ui/spinner";
import { useAuthStore } from "@/store/authStore";
import { useProfileStore } from "@/store/profileStore";
import { useTeamsStore } from "@/store/teamsStore";
import TeamFormDialog from "./TeamFormDialog";
import InvitationsBanner from "./InvitationsBanner";
import TeamDetails from "./TeamDetails";
import TeamsSidebar from "./TeamsSidebar";
import { panelClass, primaryButtonClass } from "./teamStyles";

/** Why the user cannot create a team right now, or null when they can. */
function createBlockedReason(overview: TeamsOverviewModel, pilotMissing: boolean): string | null {
    if (pilotMissing) return "Link your Velocidrone pilot first";
    if (overview.myTeam) return "You are already in a team";
    return null;
}

/**
 * Teams: anyone can browse them; a signed-in user also manages their own team and answers invitations.
 */
function TeamsPage() {
    const user = useAuthStore((state) => state.user);
    const authState = useAuthStore((state) => state.state);
    const pilotMissing = useProfileStore((state) => state.profile !== null && state.profile.pilot == null);
    const overview = useTeamsStore((state) => state.overview);
    const overviewState = useTeamsStore((state) => state.overviewState);
    const hasSelectedTeam = useTeamsStore((state) => state.selectedTeamId !== null);
    const [creating, setCreating] = useState(false);

    const firstTeamId = useTeamsStore((state) => state.otherTeams[0]?.id);

    useEffect(() => {
        useTeamsStore.getState().load();
    }, [user]);

    // A visitor has no team of their own, so the page opens on the first one in the list
    useEffect(() => {
        if (!user && !hasSelectedTeam && firstTeamId !== undefined) {
            useTeamsStore.getState().selectTeam(firstTeamId);
        }
    }, [user, hasSelectedTeam, firstTeamId]);

    if (!user && authState === "Loading") return <Spinner />;

    if (!overview) {
        if (overviewState === "Error") return <p className="mt-8 text-center text-red-400">Failed to load the teams.</p>;
        return <Spinner />;
    }

    const blockedReason = createBlockedReason(overview, pilotMissing);

    return (
        <div className="mx-auto w-full max-w-[1100px]">
            <InvitationsBanner />

            <div className="grid items-start gap-6 md:grid-cols-[300px_1fr]">
                <TeamsSidebar onCreateTeam={blockedReason === null ? () => setCreating(true) : undefined} />

                {hasSelectedTeam ? (
                    <TeamDetails />
                ) : (
                    <section className={`${panelClass} px-6 py-14 text-center`}>
                        {user ? (
                            <>
                                <h2 className="mb-7 text-xl font-semibold text-white">You're not in a team yet</h2>
                                <button
                                    onClick={() => setCreating(true)}
                                    disabled={blockedReason !== null}
                                    className={primaryButtonClass}
                                >
                                    Create a team
                                </button>
                                {pilotMissing && (
                                    <p className="mt-4 text-xs text-slate-400">
                                        <Link to="/profile" className="text-emerald-400 hover:underline">
                                            Link your Velocidrone pilot
                                        </Link>{" "}
                                        in your profile to create a team.
                                    </p>
                                )}
                            </>
                        ) : (
                            <h2 className="text-xl font-semibold text-white">No teams yet</h2>
                        )}
                    </section>
                )}
            </div>

            <TeamFormDialog open={creating} onClose={() => setCreating(false)} />
        </div>
    );
}

export default TeamsPage;
