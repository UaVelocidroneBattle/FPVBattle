import { useEffect } from "react";
import { Link } from "react-router-dom";
import { useDayStreakStore, DayStreakLeaderboardRow } from "@/store/dayStreakStore";
import { Spinner } from "@/components/ui/spinner";

const MEDAL_COLORS: Record<number, string> = {
    1: "text-amber-400",
    2: "text-slate-300",
    3: "text-orange-400",
};

function StreakRow({ row, rank }: { row: DayStreakLeaderboardRow; rank: number }) {
    const rankColor = MEDAL_COLORS[rank] ?? "text-slate-400";

    return (
        <li className="px-3 py-4 hover:bg-slate-700/30 transition-colors duration-150">
            <div className="flex items-center gap-4">
                <span className={`w-8 flex-shrink-0 font-bold tabular-nums text-lg ${rankColor}`}>
                    {String(rank).padStart(2, "0")}
                </span>

                <div className="flex-1 min-w-0">
                    <Link
                        to={`/statistics/profile/${encodeURIComponent(row.pilotName)}`}
                        className="text-sm font-medium text-slate-200 hover:text-emerald-400 transition-colors"
                    >
                        {row.pilotName}
                    </Link>
                </div>

                <div className="flex items-center w-24 justify-end flex-shrink-0">
                    <span className="text-base font-semibold text-slate-200 tabular-nums">{row.dayStreak}</span>
                </div>

                <div className="w-24 text-right flex-shrink-0">
                    <span className="text-sm text-slate-400 tabular-nums">{row.maxStreak}</span>
                </div>
            </div>
        </li>
    );
}

function DayStreaksPage() {
    const rows = useDayStreakStore((state) => state.rows);
    const loadingState = useDayStreakStore((state) => state.loadingState);

    useEffect(() => {
        const { fetchLeaderboard } = useDayStreakStore.getState();
        fetchLeaderboard();
    }, []);

    return (
        <div className="space-y-6">
            <div>
                <h2 className="text-xl font-semibold text-white mb-1">Day Streaks</h2>
                <p className="text-slate-400 text-sm">Pilots ranked by their current consecutive day streak</p>
            </div>

            {loadingState === "Loading" && (
                <div className="flex justify-center py-12">
                    <Spinner />
                </div>
            )}

            {loadingState === "Error" && (
                <p className="text-red-400 text-center py-8">Failed to load day streaks</p>
            )}

            {loadingState === "Loaded" && rows.length === 0 && (
                <p className="text-slate-400 text-center py-8">No active streaks today</p>
            )}

            {loadingState === "Loaded" && rows.length > 0 && (
                <div className="overflow-hidden -mx-6 sm:mx-0">
                    <div className="px-3 py-3 border-b border-slate-700/50 flex items-center gap-4">
                        <div className="w-8 flex-shrink-0" />
                        <div className="flex-1 text-xs font-semibold uppercase tracking-wider text-slate-500">Pilot</div>
                        <div className="w-24 text-right flex-shrink-0 text-xs font-semibold uppercase tracking-wider text-slate-500">Day Streak</div>
                        <div className="w-24 text-right flex-shrink-0 text-xs font-semibold uppercase tracking-wider text-slate-500">Max Streak</div>
                    </div>
                    <ul className="divide-y divide-slate-700/50">
                        {rows.map((row, index) => (
                            <StreakRow key={row.pilotName} row={row} rank={index + 1} />
                        ))}
                    </ul>
                </div>
            )}
        </div>
    );
}

export default DayStreaksPage;
