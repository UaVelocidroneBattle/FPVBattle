import { create } from "zustand";
import {
    getApiTeamsDetails,
    getApiTeamsOverview,
    getApiTeamsSearch,
    postApiTeamsAccept,
    postApiTeamsCreate,
    postApiTeamsDecline,
    postApiTeamsInvite,
    postApiTeamsLeave,
    postApiTeamsRemove,
    postApiTeamsRemoveMember,
    postApiTeamsRevokeInvite,
    postApiTeamsSetRole,
    postApiTeamsUpdate,
} from "@/api/client";
import type { TeamDetailsModel, TeamRole, TeamsOverviewModel, TeamSummaryModel } from "@/api/client";
import { LoadingStates } from "@/lib/loadingStates";

export interface ActionResult {
    ok: boolean;
    error?: string;
}

interface TeamsState {
    overview: TeamsOverviewModel | null;
    overviewState: LoadingStates;
    otherTeams: TeamSummaryModel[];
    searchQuery: string;
    selectedTeamId: number | null;
    details: TeamDetailsModel | null;
    detailsState: LoadingStates;
}

interface TeamsActions {
    /** Loads only what the navigation needs: the user's teams and invitations. */
    fetchOverview: () => Promise<void>;
    /** Loads everything the Teams page shows. */
    load: () => Promise<void>;
    selectTeam: (teamId: number) => Promise<void>;
    search: (query: string) => Promise<void>;
    createTeam: (name: string, tag: string, description: string) => Promise<ActionResult>;
    updateTeam: (teamId: number, name: string, tag: string, description: string) => Promise<ActionResult>;
    inviteToTeam: (teamId: number, pilotName: string) => Promise<ActionResult>;
    acceptInvitation: (teamId: number) => Promise<ActionResult>;
    declineInvitation: (teamId: number) => Promise<ActionResult>;
    leaveTeam: (teamId: number) => Promise<ActionResult>;
    removeTeam: (teamId: number) => Promise<ActionResult>;
    revokeInvitation: (teamId: number, userId: string) => Promise<ActionResult>;
    setMemberRole: (teamId: number, userId: string, role: TeamRole) => Promise<ActionResult>;
    removeMember: (teamId: number, userId: string) => Promise<ActionResult>;
    /** Drops another user's teams when the session ends. */
    reset: () => void;
}

type TeamsStore = TeamsState & TeamsActions;

const initialState: TeamsState = {
    overview: null,
    overviewState: "Idle",
    otherTeams: [],
    searchQuery: "",
    selectedTeamId: null,
    details: null,
    detailsState: "Idle",
};

const NETWORK_ERROR = "Network error. Please try again.";

interface MutationResponse {
    response: Response;
    error?: unknown;
}

function toResult(response: MutationResponse, fallbackError: string): ActionResult {
    if (response.response.ok) return { ok: true };

    const serverMessage = typeof response.error === "string" ? response.error : "";
    return { ok: false, error: serverMessage || fallbackError };
}

export const useTeamsStore = create<TeamsStore>()((set, get) => {
    const fetchOtherTeams = async () => {
        const query = get().searchQuery;

        try {
            const response = await getApiTeamsSearch({ query: { query: query || undefined } });
            // A newer search has started since: its answer is the one to keep
            if (response.data && get().searchQuery === query) set({ otherTeams: response.data });
        } catch {
            // The list keeps its previous content
        }
    };

    /** Returns false when the team no longer exists. */
    const loadDetails = async (teamId: number): Promise<boolean> => {
        const isSameTeam = get().details?.id === teamId;
        set({ selectedTeamId: teamId, ...(isSameTeam ? {} : { details: null, detailsState: "Loading" as const }) });

        try {
            const response = await getApiTeamsDetails({ query: { teamId } });
            if (get().selectedTeamId !== teamId) return true;

            if (response.data) {
                set({ details: response.data, detailsState: "Loaded" });
                return true;
            }

            set({ details: null, detailsState: response.response.status === 404 ? "Idle" : "Error" });
            return response.response.status !== 404;
        } catch {
            if (get().selectedTeamId === teamId) set({ detailsState: "Error" });
            return true;
        }
    };

    const clearSelection = () => set({ selectedTeamId: null, details: null, detailsState: "Idle" });

    /** Reloads the page data and falls back to the user's first team when the selected one is gone. */
    const refresh = async () => {
        await Promise.all([get().fetchOverview(), fetchOtherTeams()]);

        const defaultTeamId = get().overview?.myTeams[0]?.id ?? null;
        const teamId = get().selectedTeamId ?? defaultTeamId;

        if (teamId === null) return clearSelection();

        const found = await loadDetails(teamId);
        if (found) return;

        if (defaultTeamId !== null && defaultTeamId !== teamId) await loadDetails(defaultTeamId);
        else clearSelection();
    };

    /**
     * Runs a mutation and refreshes the page on success. `selectAfter` picks the team to show
     * afterwards: a team id, or null to fall back to the user's first team.
     */
    const perform = async (
        request: () => Promise<MutationResponse>,
        fallbackError: string,
        selectAfter?: number | null,
    ): Promise<ActionResult> => {
        try {
            const result = toResult(await request(), fallbackError);
            if (!result.ok) return result;

            if (selectAfter !== undefined) set({ selectedTeamId: selectAfter });
            await refresh();
            return result;
        } catch {
            return { ok: false, error: NETWORK_ERROR };
        }
    };

    return {
        ...initialState,

        fetchOverview: async () => {
            set({ overviewState: get().overview ? "Loaded" : "Loading" });
            try {
                const response = await getApiTeamsOverview();
                set(response.data ? { overview: response.data, overviewState: "Loaded" } : { overviewState: "Error" });
            } catch {
                set({ overviewState: "Error" });
            }
        },

        load: refresh,

        selectTeam: async (teamId) => {
            await loadDetails(teamId);
        },

        search: async (query) => {
            set({ searchQuery: query });
            await fetchOtherTeams();
        },

        createTeam: async (name, tag, description) => {
            try {
                const response = await postApiTeamsCreate({ body: { name, tag, description: description || null } });
                if (!response.data) return toResult(response, "Could not create the team. Please try again.");

                set({ selectedTeamId: response.data.id });
                await refresh();
                return { ok: true };
            } catch {
                return { ok: false, error: NETWORK_ERROR };
            }
        },

        updateTeam: (teamId, name, tag, description) =>
            perform(
                () => postApiTeamsUpdate({ body: { teamId, name, tag, description: description || null } }),
                "Could not save the team. Please try again.",
            ),

        inviteToTeam: (teamId, pilotName) =>
            perform(
                () => postApiTeamsInvite({ body: { teamId, pilotName } }),
                "Could not send the invite. Please try again.",
            ),

        acceptInvitation: (teamId) =>
            perform(() => postApiTeamsAccept({ body: { teamId } }), "Could not accept the invitation.", teamId),

        declineInvitation: (teamId) =>
            perform(() => postApiTeamsDecline({ body: { teamId } }), "Could not decline the invitation."),

        leaveTeam: (teamId) =>
            perform(() => postApiTeamsLeave({ body: { teamId } }), "Could not leave the team.", null),

        removeTeam: (teamId) =>
            perform(() => postApiTeamsRemove({ body: { teamId } }), "Could not delete the team.", null),

        revokeInvitation: (teamId, userId) =>
            perform(() => postApiTeamsRevokeInvite({ body: { teamId, userId } }), "Could not revoke the invite."),

        setMemberRole: (teamId, userId, role) =>
            perform(() => postApiTeamsSetRole({ body: { teamId, userId, role } }), "Could not change the role."),

        removeMember: (teamId, userId) =>
            perform(() => postApiTeamsRemoveMember({ body: { teamId, userId } }), "Could not remove the member."),

        reset: () => set(initialState),
    };
});
