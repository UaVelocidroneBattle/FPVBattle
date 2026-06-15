import { LeagueLeaderboardModel } from "../api/client";
import { convertMsToSec } from "../utils/utils";
import PilotName from "@/components/PilotName";
import CountryFlag from "@/components/ui/CountryFlag";

interface CurrentLeaderboardProps {
    leaderboard: LeagueLeaderboardModel[];
    leagueColors?: Map<string, string>;
    flat?: boolean;
}

const COLS = "md:grid-cols-[2.5rem_1fr_auto_2rem_5rem] grid-cols-[2.5rem_1fr_2rem_5rem]";

function rankStyle(localRank: number): string {
    if (localRank === 1) return "font-bold text-yellow-500";
    if (localRank === 2) return "font-bold text-slate-400";
    if (localRank === 3) return "font-bold text-amber-700";
    return "font-medium text-slate-500";
}

function ColumnHeaders() {
    return (
        <div className={`px-4 py-2 border-b border-slate-700 grid ${COLS} gap-6`}>
            <div className="text-xs font-medium text-slate-500 text-right">#</div>
            <div className="text-xs font-medium text-slate-500">Pilot</div>
            <div className="hidden md:block text-xs font-medium text-slate-500">Quad</div>
            <div />
            <div className="text-xs font-medium text-slate-500 text-right">Time</div>
        </div>
    );
}

function CurrentLeaderboard({ leaderboard, leagueColors, flat = false }: CurrentLeaderboardProps) {
    const isEmpty = !leaderboard?.length || leaderboard.every(g => !g.results?.length);

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
            <div className="bg-slate-800/50 backdrop-blur-sm border border-slate-700 overflow-hidden">
                <ColumnHeaders />
                <ul>
                    {results.map((result, index) => {
                        const leagueColor = (result.league && leagueColors?.get(result.league)) || '#34d399';
                        return (
                            <li
                                key={`${result.playerName}-${index}`}
                                className={`px-4 py-3 hover:bg-slate-600/20 transition-colors duration-150 border-l-4 ${index % 2 === 0 ? "bg-slate-700/20" : ""}`}
                                style={{ borderLeftColor: leagueColor }}
                            >
                                <div className={`grid ${COLS} items-center gap-6`}>
                                    <span className={`text-right text-sm tabular-nums ${rankStyle(index + 1)}`}>
                                        {String(index + 1).padStart(2, "0")}
                                    </span>
                                    <PilotName name={result.playerName} className="text-sm text-slate-200 truncate" />
                                    <p className="hidden md:block text-sm text-slate-400 truncate">{result.modelName}</p>
                                    <CountryFlag countryCode={result.country} className="text-sm" />
                                    <div className="text-sm font-semibold text-slate-200 tabular-nums text-right">
                                        {convertMsToSec(result.trackTime ?? 0)}
                                    </div>
                                </div>
                            </li>
                        );
                    })}
                </ul>
            </div>
        );
    }

    const hasLeagues = leaderboard.length > 1;

    return (
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
                            <ColumnHeaders />
                            <ul>
                                {group.results.map((result, index) => (
                                    <li
                                        key={`${result.playerName}-${result.localRank}`}
                                        className={`px-4 py-3 hover:bg-slate-600/20 transition-colors duration-150 ${index % 2 === 0 ? "bg-slate-700/20" : ""}`}
                                    >
                                        <div className={`grid ${COLS} items-center gap-6`}>
                                            <span className={`text-right text-sm tabular-nums ${rankStyle(result.localRank ?? 0)}`}>
                                                {String(result.localRank ?? 0).padStart(2, "0")}
                                            </span>
                                            <PilotName name={result.playerName} className="text-sm text-slate-200 truncate" />
                                            <p className="hidden md:block text-sm text-slate-400 truncate">{result.modelName}</p>
                                            <CountryFlag countryCode={result.country} className="text-sm" />
                                            <div className="text-sm font-semibold text-slate-200 tabular-nums text-right">
                                                {convertMsToSec(result.trackTime ?? 0)}
                                            </div>
                                        </div>
                                    </li>
                                ))}
                            </ul>
                        </>
                    )}
                </div>
            ))}
        </div>
    );
}

export default CurrentLeaderboard;
