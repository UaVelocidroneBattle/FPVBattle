import { useEffect } from "react";
import { useShallow } from "zustand/shallow";
import { getTeamCompetitionStore } from "@/store/teamCompetitionStore";
import { useTeamsStore } from "@/store/teamsStore";
import { Spinner } from "@/components/ui/spinner.tsx";
import { Error } from "@/components/ui/error.tsx";
import { bleedOnPhones, boardTitleRow } from "@/lib/layout";
import TeamLeaderboard from "./TeamLeaderboard";
import TeamSeasonLeaderboard from "./TeamSeasonLeaderboard";

const TEN_MINUTES = 10 * 60 * 1000;

interface TeamBoardsProps {
    cupId: string;
    selectedDate: string | null;
    isEnded: boolean;
}

function BoardTitle({ title, count }: { title: string; count: number }) {
    return (
        <div className={`flex flex-col justify-center ${boardTitleRow}`}>
            <h3 className="text-sm uppercase tracking-wider text-emerald-400 font-medium px-1 flex justify-center sm:justify-start items-baseline gap-2">
                {title}
                {count > 0 && (
                    <span className="hidden sm:inline text-xs text-slate-500 normal-case tracking-normal font-normal">{count} teams</span>
                )}
            </h3>
        </div>
    );
}

/** Today's and the season's team leaderboards, shown in place of the pilots' ones in team mode. */
function TeamBoards({ cupId, selectedDate, isEnded }: TeamBoardsProps) {
    const { state, data } = getTeamCompetitionStore(cupId)(
        useShallow((state) => ({ state: state.state, data: state.data }))
    );
    const myTeamId = useTeamsStore((state) => state.overview?.myTeam?.id);

    useEffect(() => {
        getTeamCompetitionStore(cupId).getState().load(selectedDate);
    }, [cupId, selectedDate]);

    useEffect(() => {
        const interval = setInterval(() => {
            getTeamCompetitionStore(cupId).getState().refresh();
        }, TEN_MINUTES);
        return () => clearInterval(interval);
    }, [cupId]);

    if (state === "Error") return <Error />;
    if (state !== "Loaded" || data === null) return <Spinner />;

    return (
        <div className="grid lg:grid-cols-2 gap-8">
            <div className="flex flex-col gap-3">
                <BoardTitle title="TODAY'S TEAMS" count={data.leaderboard.length} />
                <div className={bleedOnPhones}>
                    <TeamLeaderboard results={data.leaderboard} isEnded={isEnded} myTeamId={myTeamId} />
                </div>
            </div>

            <div className="flex flex-col gap-3">
                <BoardTitle title="SEASON TEAMS" count={data.seasonLeaderboard.length} />
                <div className={bleedOnPhones}>
                    <TeamSeasonLeaderboard results={data.seasonLeaderboard} myTeamId={myTeamId} />
                </div>
            </div>
        </div>
    );
}

export default TeamBoards;
