import { useState } from "react";
import { LeagueSeasonLeaderboard, SeasonResult } from "../api/client";
import { ChevronDown } from "lucide-react";
import { Spinner } from "@/components/ui/spinner.tsx";
import PilotName from "@/components/PilotName";
import CountryFlag from "@/components/ui/CountryFlag";

interface LeaderBoardProps {
    leaderboard: LeagueSeasonLeaderboard[];
    leagueColors?: Map<string, string>;
    /** The signed-in user's linked pilot name, ordinally matched to highlight their row. */
    highlightPilotName?: string | null;
}

const PAGE_SIZE = 25;

function rankStyle(rank: number): string {
    if (rank === 1) return "font-bold text-yellow-500";
    if (rank === 2) return "font-bold text-slate-400";
    if (rank === 3) return "font-bold text-amber-700";
    return "font-medium text-slate-500";
}

function ColumnHeaders() {
    return (
        <div className="px-4 py-2 border-b border-slate-700 grid grid-cols-[2.5rem_1fr_2rem_4rem] gap-6">
            <div className="text-xs font-medium text-slate-500 text-right">#</div>
            <div className="text-xs font-medium text-slate-500">Pilot</div>
            <div />
            <div className="text-xs font-medium text-slate-500 text-right">Points</div>
        </div>
    );
}

function ResultRow({ result, index, isHighlighted }: { result: SeasonResult; index: number; isHighlighted: boolean }) {
    return (
        <li
            className={`px-4 py-3 hover:bg-slate-600/20 transition-colors duration-150 ${
                isHighlighted ? "bg-emerald-500/10 border-y border-emerald-400/40" : index % 2 === 0 ? "bg-slate-700/20" : ""
            }`}
        >
            <div className="grid grid-cols-[2.5rem_1fr_2rem_4rem] items-center gap-6">
                <span className={`text-right text-sm tabular-nums ${rankStyle(result.rank ?? 0)}`}>
                    {String(result.rank).padStart(2, "0")}
                </span>
                <PilotName name={result.playerName} className="text-sm text-slate-200 truncate" />
                <CountryFlag countryCode={result.country} className="text-sm" />
                <div className="text-sm font-semibold text-slate-200 tabular-nums text-right">{result.points}</div>
            </div>
        </li>
    );
}

function LeagueGroup({ group, hasLeagues, leagueColors, highlightPilotName }: { group: LeagueSeasonLeaderboard; hasLeagues: boolean; leagueColors?: Map<string, string>; highlightPilotName?: string | null }) {
    const [showMore, setShowMore] = useState(false);
    const first = group.results?.slice(0, PAGE_SIZE) ?? [];
    const rest = group.results?.slice(PAGE_SIZE) ?? [];

    return (
        <div className="bg-slate-800/50 backdrop-blur-sm border border-slate-700 overflow-hidden">
            {hasLeagues && (
                <div className="px-4 py-2 border-b border-slate-700 bg-slate-700/20">
                    <span
                        className="text-xs font-semibold uppercase tracking-wider text-emerald-400"
                        style={{ color: (group.league && leagueColors?.get(group.league)) || undefined }}
                    >
                        {group.league ?? "Others"}
                    </span>
                </div>
            )}
            {!group.results?.length ? (
                <div className="px-4 py-6 text-slate-500 text-sm text-center">No results</div>
            ) : (
                <>
                    <ColumnHeaders />
                    <ul>
                        {first.map((result, index) => (
                            <ResultRow key={result.playerName} result={result} index={index} isHighlighted={result.playerName === highlightPilotName} />
                        ))}
                        {showMore && rest.map((result, index) => (
                            <ResultRow key={result.playerName} result={result} index={index + PAGE_SIZE} isHighlighted={result.playerName === highlightPilotName} />
                        ))}
                    </ul>
                </>
            )}
            {rest.length > 0 && !showMore && (
                <div className="flex justify-center py-4">
                    <button
                        className="flex items-center text-emerald-400 hover:text-emerald-300 transition-colors text-sm font-medium"
                        onClick={() => setShowMore(true)}
                    >
                        Load more <ChevronDown className="ml-2 h-4 w-4" />
                    </button>
                </div>
            )}
        </div>
    );
}

function LeaderBoard({ leaderboard, leagueColors, highlightPilotName }: LeaderBoardProps) {
    if (!leaderboard) return <Spinner />;
    if (!leaderboard.length || leaderboard.every(g => !g.results?.length)) {
        return (
            <div className="bg-slate-800/50 backdrop-blur-sm border border-slate-700 px-6 py-8 text-slate-400 text-sm">
                No results yet
            </div>
        );
    }

    const hasLeagues = leaderboard.length > 1;

    return (
        <div className={hasLeagues ? "flex flex-col gap-6" : ""}>
            {leaderboard.map(group => (
                <LeagueGroup key={group.league ?? 'all'} group={group} hasLeagues={hasLeagues} leagueColors={leagueColors} highlightPilotName={highlightPilotName} />
            ))}
        </div>
    );
}

export default LeaderBoard;
