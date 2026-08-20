import { LeaderboardResultModel, LeagueLeaderboardModel } from "../api/client";
import { convertMsToSec } from "../utils/utils";
import PilotName from "@/components/PilotName";
import CountryFlag from "@/components/ui/CountryFlag";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";

/** A result of a filtered board keeps the position it held on the full one. */
export interface LeaderboardResult extends LeaderboardResultModel {
    originalRank?: number;
}

export interface LeagueLeaderboard extends Omit<LeagueLeaderboardModel, "results"> {
    results: LeaderboardResult[];
}

interface CurrentLeaderboardProps {
    leaderboard: LeagueLeaderboard[];
    leagueColors?: Map<string, string>;
    flat?: boolean;
    isEnded?: boolean;
    /** The signed-in user's linked pilot name, ordinally matched to highlight their row. */
    highlightPilotName?: string | null;
}

function rowHighlightClass(isHighlighted: boolean, isEvenRow: boolean): string {
    if (isHighlighted) return "bg-emerald-500/10 border-y border-emerald-400/40";
    return isEvenRow ? "bg-slate-700/20" : "";
}

/**
 * Written out in full rather than composed, so Tailwind can see every class it
 * has to generate. The wide variants give the position column room for the rank
 * a pilot held before the board was filtered.
 */
const COLUMNS = {
    running: "md:grid-cols-[2.5rem_1fr_auto_2rem_5rem] grid-cols-[2.5rem_1fr_2rem_5rem]",
    runningWide: "md:grid-cols-[4.5rem_1fr_auto_2rem_5rem] grid-cols-[4.5rem_1fr_2rem_5rem]",
    ended: "md:grid-cols-[2.5rem_1fr_auto_2rem_5rem_3.5rem] grid-cols-[2.5rem_1fr_2rem_5rem_3.5rem]",
    endedWide: "md:grid-cols-[4.5rem_1fr_auto_2rem_5rem_3.5rem] grid-cols-[4.5rem_1fr_2rem_5rem_3.5rem]",
};

function columnsFor(isEnded: boolean, withOriginalRanks: boolean): string {
    if (isEnded) return withOriginalRanks ? COLUMNS.endedWide : COLUMNS.ended;
    return withOriginalRanks ? COLUMNS.runningWide : COLUMNS.running;
}

const hasOriginalRanks = (leaderboard: LeagueLeaderboard[]) =>
    leaderboard.some(group => group.results?.some(result => result.originalRank !== undefined));

function rankStyle(localRank: number): string {
    if (localRank === 1) return "font-bold text-yellow-500";
    if (localRank === 2) return "font-bold text-slate-400";
    if (localRank === 3) return "font-bold text-amber-700";
    return "font-medium text-slate-500";
}

const formatRank = (rank: number) => String(rank).padStart(2, "0");

/** Gap to the average time of the top pilots, e.g. "15.22%". Null when there aren't enough pilots yet. */
function formatGapToLeader(gapToLeaderPercent?: number | string | null): string | null {
    if (gapToLeaderPercent == null) return null;
    return `${Number(gapToLeaderPercent).toFixed(2)}%`;
}

/** A pilot's finish time; hovering it reveals their gap to the leader, when there's enough data to know it. */
function TimeCell({ trackTimeMs, gapToLeaderPercent }: { trackTimeMs: number; gapToLeaderPercent?: number | string | null }) {
    const time = convertMsToSec(trackTimeMs);
    const gap = formatGapToLeader(gapToLeaderPercent);

    if (gap === null) {
        return <div className="text-sm font-semibold text-slate-200 tabular-nums text-right">{time}</div>;
    }

    return (
        <Tooltip>
            <TooltipTrigger asChild>
                <div className="text-sm font-semibold text-slate-200 tabular-nums text-right cursor-default">
                    {time}
                </div>
            </TooltipTrigger>
            <TooltipContent className="bg-slate-800 border-slate-700">
                <span className="text-slate-400">Gap to leader:</span>{" "}
                <span className="font-semibold text-slate-100 ml-1">{gap}</span>
            </TooltipContent>
        </Tooltip>
    );
}

/**
 * A filtered board is numbered from one again, with the position from the full
 * board kept alongside, e.g. "01 (07)" — as the country leaderboards of the
 * global rating do it.
 */
function Position({ rank, originalRank }: { rank: number; originalRank?: number }) {
    return (
        <span className={`text-right text-sm tabular-nums whitespace-nowrap ${rankStyle(rank)}`}>
            {formatRank(rank)}
            {originalRank !== undefined && (
                <span className="ml-1 text-xs font-medium text-slate-500">({formatRank(originalRank)})</span>
            )}
        </span>
    );
}

function ColumnHeaders({ isEnded, cols }: { isEnded: boolean; cols: string }) {
    return (
        <div className={`px-4 py-2 border-b border-slate-700 grid ${cols} gap-6`}>
            <div className="text-xs font-medium text-slate-500 text-right">#</div>
            <div className="text-xs font-medium text-slate-500">Pilot</div>
            <div className="hidden md:block text-xs font-medium text-slate-500">Quad</div>
            <div />
            <div className="text-xs font-medium text-slate-500 text-right">Time</div>
            {isEnded && <div className="text-xs font-medium text-slate-500 text-right">Pts</div>}
        </div>
    );
}

function CurrentLeaderboard({ leaderboard, leagueColors, flat = false, isEnded = false, highlightPilotName }: CurrentLeaderboardProps) {
    const isEmpty = !leaderboard?.length || leaderboard.every(g => !g.results?.length);
    const cols = columnsFor(isEnded, hasOriginalRanks(leaderboard));

    if (isEmpty) {
        return (
            <div className="bg-slate-800/50 backdrop-blur-sm border border-slate-700 px-4 py-8 text-slate-400 text-sm">
                No results yet
            </div>
        );
    }

    if (flat) {
        const results = leaderboard
            .flatMap(g => (g.results ?? []).map(r => ({ ...r, league: g.league ?? null })))
            .sort((a, b) => (a.trackTime ?? 0) - (b.trackTime ?? 0));

        return (
            <TooltipProvider delayDuration={150}>
                <div className="bg-slate-800/50 backdrop-blur-sm border border-slate-700 overflow-hidden">
                    <ColumnHeaders isEnded={isEnded} cols={cols} />
                    <ul>
                        {results.map((result, index) => {
                            const leagueColor = (result.league && leagueColors?.get(result.league)) || '#34d399';
                            const isHighlighted = result.playerName === highlightPilotName;
                            return (
                                <li
                                    key={`${result.playerName}-${index}`}
                                    className={`px-4 py-3 hover:bg-slate-600/20 transition-colors duration-150 border-l-4 ${rowHighlightClass(isHighlighted, index % 2 === 0)}`}
                                    style={{ borderLeftColor: leagueColor }}
                                >
                                    <div className={`grid ${cols} items-center gap-6`}>
                                        <Position rank={index + 1} originalRank={result.originalRank} />
                                        <PilotName name={result.playerName} className="text-sm text-slate-200 truncate" />
                                        <p className="hidden md:block text-sm text-slate-400 truncate">{result.modelName}</p>
                                        <CountryFlag countryCode={result.country} className="text-sm" />
                                        <TimeCell trackTimeMs={result.trackTime ?? 0} gapToLeaderPercent={result.gapToLeaderPercent} />
                                        {isEnded && (
                                            <div className="text-sm font-semibold text-emerald-400 tabular-nums text-right">
                                                {result.points ?? "—"}
                                            </div>
                                        )}
                                    </div>
                                </li>
                            );
                        })}
                    </ul>
                </div>
            </TooltipProvider>
        );
    }

    const hasLeagues = leaderboard.length > 1;

    return (
        <TooltipProvider delayDuration={150}>
            <div className={hasLeagues ? "flex flex-col gap-6" : ""}>
                {leaderboard.map(group => (
                    <div key={group.league ?? 'all'} className="bg-slate-800/50 backdrop-blur-sm border border-slate-700 overflow-hidden">
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
                                <ColumnHeaders isEnded={isEnded} cols={cols} />
                                <ul>
                                    {group.results.map((result, index) => {
                                        const isHighlighted = result.playerName === highlightPilotName;
                                        return (
                                            <li
                                                key={`${result.playerName}-${result.localRank}`}
                                                className={`px-4 py-3 hover:bg-slate-600/20 transition-colors duration-150 ${rowHighlightClass(isHighlighted, index % 2 === 0)}`}
                                            >
                                                <div className={`grid ${cols} items-center gap-6`}>
                                                    <Position rank={result.localRank ?? 0} originalRank={result.originalRank} />
                                                    <PilotName name={result.playerName} className="text-sm text-slate-200 truncate" />
                                                    <p className="hidden md:block text-sm text-slate-400 truncate">{result.modelName}</p>
                                                    <CountryFlag countryCode={result.country} className="text-sm" />
                                                    <TimeCell trackTimeMs={result.trackTime ?? 0} gapToLeaderPercent={result.gapToLeaderPercent} />
                                                    {isEnded && (
                                                        <div className="text-sm font-semibold text-emerald-400 tabular-nums text-right">
                                                            {result.points ?? "—"}
                                                        </div>
                                                    )}
                                                </div>
                                            </li>
                                        );
                                    })}
                                </ul>
                            </>
                        )}
                    </div>
                ))}
            </div>
        </TooltipProvider>
    );
}

export default CurrentLeaderboard;
