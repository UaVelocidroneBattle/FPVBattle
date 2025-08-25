import { TrackTimeModel } from "../api/client";
import { convertMsToSec } from "../utils/utils";

interface CurrentLeaderboardProps {
    trackResults: TrackTimeModel[];
}

const CurrentLeaderboard: React.FC<CurrentLeaderboardProps> = ({ trackResults }: CurrentLeaderboardProps) => {

    if (!trackResults || !trackResults.length) return <></>;

    return (
        <div className="overflow-hidden">
            <ul className="divide-y divide-slate-700/50">
                {trackResults.map((pilot, index) => (
                    <li key={pilot.playerName} className="px-6 py-4 hover:bg-slate-700/30 transition-colors duration-150">
                        <div className="grid md:grid-cols-[2rem_1fr_auto_7rem] grid-cols-[2rem_1fr_5rem] items-center gap-4">
                            {/* Rank */}
                            <span className="text-right font-bold text-slate-400 text-2xl tabular-nums">
                                {String(index + 1).padStart(2, "0")}
                            </span>

                            {/* Player name */}
                            <p className="truncate text-sm font-medium text-slate-200">
                                {pilot.playerName}
                            </p>

                            {/* Model */}
                            <p className="hidden md:block text-sm font-medium text-slate-200">
                                {pilot.modelName}
                            </p>

                            {/* Time */}
                            <div className="text-lg text-slate-300 font-semibold tabular-nums text-right">
                                {convertMsToSec(pilot.time)}
                            </div>
                        </div>
                    </li>
                ))}
            </ul>
        </div>
    )
}

export default CurrentLeaderboard;