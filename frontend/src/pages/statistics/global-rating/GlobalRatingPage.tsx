import { useEffect } from "react";
import { ArrowDown, ArrowUp } from "lucide-react";
import { useGlobalRatingStore, PilotRatingModel } from "@/store/globalRatingStore";
import { Spinner } from "@/components/ui/spinner";
import PilotName from "@/components/PilotName";
import CountryFlag from "@/components/ui/CountryFlag";
import { formatDate } from "@/lib/utils";

const CUP_ID = "open-class";


function RankChange({ change }: { change: number | null }) {
    if (change === null)
        return (
            <span className="absolute top-full mt-0.4 left-0 text-[10px] font-bold text-amber-400 tracking-wide">
                NEW
            </span>
        );
    if (change === 0) return null;
    const improved = change < 0;
    const Icon = improved ? ArrowUp : ArrowDown;
    return (
        <span className={`absolute top-full mt-0.4 left-0 flex items-center gap-0.5 text-xs font-medium ${improved ? "text-emerald-400" : "text-red-400"}`}>
            <Icon className="h-3 w-3" />
            {Math.abs(change)}
        </span>
    );
}

function GapChange({ change }: { change: number }) {
    if (change === 0) return null;
    const improved = change < 0;
    const Icon = improved ? ArrowUp : ArrowDown;
    return (
        <span className={`absolute top-full mt-0.4 right-0 flex items-center gap-0.5 text-xs font-medium ${improved ? "text-emerald-400" : "text-red-400"}`}>
            <Icon className="h-3 w-3" />
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
            <div className="grid grid-cols-[2.5rem_1fr_2rem_5rem] items-center gap-4">
                <div className="relative flex justify-start">
                    <span className="font-bold text-slate-400 text-lg sm:text-2xl tabular-nums">
                        {String(pilot.rank).padStart(2, "0")}
                    </span>
                    <RankChange change={pilot.rankChange} />
                </div>

                <PilotName
                    name={pilot.pilotName}
                    className="text-sm font-medium text-slate-200"
                />

                <CountryFlag countryCode={pilot.country ?? ""} className="text-sm" />

                <div className="relative flex justify-end">
                    <span className="text-lg text-slate-300 font-semibold tabular-nums">
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
                    <div className="px-3 py-4 border-b border-slate-700/50 grid grid-cols-[2.5rem_1fr_2rem_5rem] gap-4">
                        <div />
                        <div className="text-sm font-medium text-emerald-400">Pilot</div>
                        <div />
                        <div className="text-sm font-medium text-emerald-400 text-right">Gap</div>
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
