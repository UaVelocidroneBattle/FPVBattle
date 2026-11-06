import { TeamSeasonResultModel } from "@/api/client";
import TeamName from "@/components/TeamName";
import { rankStyle } from "@/lib/rankStyle";

interface TeamSeasonLeaderboardProps {
    results: TeamSeasonResultModel[];
    myTeamId?: number;
}

const COLUMNS = "grid-cols-[1.5rem_1fr_3.5rem] gap-3 md:grid-cols-[2.5rem_1fr_4rem] md:gap-6";

/** The month's team points, the sum of their daily points. */
function TeamSeasonLeaderboard({ results, myTeamId }: TeamSeasonLeaderboardProps) {
    if (results.length === 0) {
        return (
            <div className="bg-slate-800/50 backdrop-blur-sm border-y sm:border border-slate-700 px-6 py-8 text-slate-400 text-sm">
                No results yet
            </div>
        );
    }

    return (
        <div className="bg-slate-800/50 backdrop-blur-sm border-y sm:border border-slate-700 overflow-hidden">
            <div className={`px-4 py-2 border-b border-slate-700 grid ${COLUMNS}`}>
                <div className="text-xs font-medium text-slate-500 text-right">#</div>
                <div className="text-xs font-medium text-slate-500">Team</div>
                <div className="text-xs font-medium text-slate-500 text-right">Points</div>
            </div>
            <ul>
                {results.map((result, index) => (
                    <li
                        key={result.teamId}
                        className={`px-4 py-3 grid ${COLUMNS} items-center hover:bg-slate-600/20 transition-colors duration-150 ${
                            result.teamId === myTeamId ? "bg-emerald-500/10 border-y border-emerald-400/40" : index % 2 === 0 ? "bg-slate-700/20" : ""
                        }`}
                    >
                        <span className={`text-right text-sm tabular-nums ${rankStyle(result.rank ?? 0)}`}>
                            {String(result.rank).padStart(2, "0")}
                        </span>
                        <TeamName teamId={result.teamId} name={result.teamName} tag={result.teamTag} isDeleted={result.isDeleted} className="text-sm text-slate-200" />
                        <span className="text-sm font-semibold text-slate-200 tabular-nums text-right">{result.points}</span>
                    </li>
                ))}
            </ul>
        </div>
    );
}

export default TeamSeasonLeaderboard;
