import { useEffect } from "react";
import { ArrowDown, ArrowUp } from "lucide-react";
import { useGlobalRatingStore, PilotRatingModel } from "@/store/globalRatingStore";
import { Spinner } from "@/components/ui/spinner";
import PilotWithAvatar from "@/components/PilotWithAvatar";
import { formatDate } from "@/lib/utils";

const CUP_ID = "open-class";


function RankChange({ change }: { change: number | null }) {
    if (change === null)
        return (
            <span className="absolute top-full mt-0.5 left-0 text-[10px] font-bold text-amber-400 tracking-wide">
                NEW
            </span>
        );
    if (change === 0) return null;
    const improved = change < 0;
    const Icon = improved ? ArrowUp : ArrowDown;
    return (
        <span className={`absolute top-full mt-0.5 left-0 flex items-center gap-0.5 text-xs font-medium ${improved ? "text-emerald-400" : "text-red-400"}`}>
            <Icon className="h-3 w-3" />
            {Math.abs(change)}
        </span>
    );
}

function GapChange({ change }: { change: number }) {
    if (change === 0) return null;
    const improved = change < 0;
    return (
        <span className={`absolute top-full mt-0.5 right-0 text-xs font-medium ${improved ? "text-emerald-400" : "text-red-400"}`}>
            {improved ? "−" : "+"}
            {Math.abs(change).toFixed(2)}%
        </span>
    );
}

function formatGap(value: number | null): string {
    if (value === null) return "—";
    const prefix = value > 0 ? "+" : "";
    return `${prefix}${value.toFixed(2)}%`;
}

function RatingRow({ pilot }: { pilot: PilotRatingModel }) {
    return (
        <li className="px-3 py-6 hover:bg-slate-700/30 transition-colors duration-150">
            <div className="flex items-center gap-4">
                <div className="relative w-8 flex-shrink-0">
                    <span className="font-bold tabular-nums text-lg sm:text-2xl text-slate-400">
                        {String(pilot.rank).padStart(2, "0")}
                    </span>
                    <RankChange change={pilot.rankChange} />
                </div>

                <div className="flex-1 min-w-0">
                    <PilotWithAvatar name={pilot.pilotName} countryCode={pilot.country ?? null} />
                </div>

                <div className="relative flex-shrink-0 text-right">
                    <span className="text-lg font-semibold text-slate-300 tabular-nums">
                        {formatGap(pilot.averageGapPercent)}
                    </span>
                    <GapChange change={pilot.averageGapChange ?? 0} />
                </div>
            </div>
        </li>
    );
}

function GlobalRatingPage() {
    const data = useGlobalRatingStore((state) => state.data);
    const loadingState = useGlobalRatingStore((state) => state.loadingState);

    useEffect(() => {
        const { fetchRatings } = useGlobalRatingStore.getState();
        fetchRatings(CUP_ID);
    }, []);

    return (
        <div className="space-y-6">
            <div>
                <h2 className="text-xl font-semibold text-white mb-1">Global Rating</h2>
                {data && (
                    <p className="text-slate-400 text-sm">
                        Last update: {formatDate(data.calculatedOn)}
                    </p>
                )}
            </div>

            {loadingState === "Loading" && (
                <div className="flex justify-center py-12">
                    <Spinner />
                </div>
            )}

            {loadingState === "Error" && (
                <p className="text-red-400 text-center py-8">Failed to load ratings</p>
            )}

            {loadingState === "Loaded" && data && (
                <div className="overflow-hidden -mx-6 sm:mx-0">
                    <div className="px-3 py-3 border-b border-slate-700/50 flex items-center gap-4">
                        <div className="w-8 flex-shrink-0" />
                        <div className="flex-1 text-xs font-semibold uppercase tracking-wider text-slate-500">Pilot</div>
                        <div className="flex-shrink-0 text-xs font-semibold uppercase tracking-wider text-slate-500">Gap</div>
                    </div>
                    <ul className="divide-y divide-slate-700/50">
                        {data.ratings.map((pilot) => (
                            <RatingRow key={pilot.pilotId} pilot={pilot} />
                        ))}
                    </ul>
                </div>
            )}
        </div>
    );
}

export default GlobalRatingPage;
