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
                        <div className="flex items-center justify-between">
                            <div className="flex items-center">
                                <span className="font-bold text-slate-400 mr-4 w-8 text-right text-2xl tabular-nums">
                                    {String(index + 1).padStart(2, '0')}
                                </span>
                                <p className="text-sm font-medium text-slate-200">
                                    {pilot.playerName}
                                </p>
                            </div>
                            <div className="text-lg text-slate-300 font-semibold tabular-nums">{convertMsToSec(pilot.time)}</div>
                        </div>
                    </li>
                ))}
            </ul>
        </div>
    )
}

export default CurrentLeaderboard;