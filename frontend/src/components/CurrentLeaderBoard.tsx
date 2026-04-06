import { TrackTimeModel } from "../api/client";
import { convertMsToSec } from "../utils/utils";
import PilotName from "@/components/PilotName";
import CountryFlag from "@/components/ui/CountryFlag";

interface CurrentLeaderboardProps {
    trackResults: TrackTimeModel[];
}

function CurrentLeaderboard({ trackResults }: CurrentLeaderboardProps) {
    if (!trackResults || !trackResults.length) {
        return (
            <div className="px-4 py-8 text-slate-400 text-sm">
                No results yet
            </div>
        );
    }

    return (
        <div className="overflow-hidden">
            <div className="px-4 py-2 border-b border-slate-700 grid md:grid-cols-[2.5rem_1fr_auto_2rem_5rem] grid-cols-[2.5rem_1fr_2rem_5rem] gap-6">
                <div className="text-xs font-medium text-slate-500 text-right">#</div>
                <div className="text-xs font-medium text-slate-500">Pilot</div>
                <div className="hidden md:block text-xs font-medium text-slate-500">Quad</div>
                <div />
                <div className="text-xs font-medium text-slate-500 text-right">Time</div>
            </div>
            <ul>
                {trackResults.map((pilot, index) => (
                    <li
                        key={pilot.playerName}
                        className={`px-4 py-3 hover:bg-slate-600/20 transition-colors duration-150 ${index % 2 === 0 ? "bg-slate-700/20" : ""}`}
                    >
                        <div className="grid md:grid-cols-[2.5rem_1fr_auto_2rem_5rem] grid-cols-[2.5rem_1fr_2rem_5rem] items-center gap-6">
                            <span className={`text-right text-sm tabular-nums ${index === 0 ? "font-bold text-yellow-500" : index === 1 ? "font-bold text-slate-400" : index === 2 ? "font-bold text-amber-700" : "font-medium text-slate-500"}`}>
                                {String(index + 1).padStart(2, "0")}
                            </span>
                            <PilotName name={pilot.playerName} className="text-sm text-slate-200 truncate" />
                            <p className="hidden md:block text-sm text-slate-400 truncate">{pilot.modelName}</p>
                            <CountryFlag countryCode={pilot.country} className="text-sm" />
                            <div className="text-sm font-semibold text-slate-200 tabular-nums text-right">
                                {convertMsToSec(pilot.time)}
                            </div>
                        </div>
                    </li>
                ))}
            </ul>
        </div>
    );
}

export default CurrentLeaderboard;
