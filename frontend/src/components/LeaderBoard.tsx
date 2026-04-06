import { useState } from "react";
import { SeasonResultModel } from "../api/client";
import { ChevronDown } from "lucide-react";
import { Spinner } from "@/components/ui/spinner.tsx";
import PilotName from "@/components/PilotName";
import CountryFlag from "@/components/ui/CountryFlag";

interface LeaderBoardProps {
    leaderBoard: SeasonResultModel[];
}

function LeaderBoard({ leaderBoard }: LeaderBoardProps) {
    const [showMore, setShowMore] = useState(false);

    if (!leaderBoard) return <Spinner />;
    if (!leaderBoard.length) return <div className="px-6 py-8 text-slate-400 text-sm">No results yet</div>;

    const first25 = leaderBoard.slice(0, 25);
    const remaining = leaderBoard.slice(25);

    return (
        <div className="overflow-hidden">
            <div className="px-4 py-2 border-b border-slate-700 grid grid-cols-[2.5rem_1fr_2rem_4rem] gap-6">
                <div className="text-xs font-medium text-slate-500 text-right">#</div>
                <div className="text-xs font-medium text-slate-500">Pilot</div>
                <div />
                <div className="text-xs font-medium text-slate-500 text-right">Points</div>
            </div>
            <ul>
                {first25.map((res, index) => (
                    <li
                        key={res.playerName}
                        className={`px-4 py-3 hover:bg-slate-600/20 transition-colors duration-150 ${index % 2 === 0 ? "bg-slate-700/20" : ""}`}
                    >
                        <div className="grid grid-cols-[2.5rem_1fr_2rem_4rem] items-center gap-6">
                            <span className={`text-right text-sm tabular-nums ${index === 0 ? "font-bold text-yellow-500" : index === 1 ? "font-bold text-slate-400" : index === 2 ? "font-bold text-amber-700" : "font-medium text-slate-500"}`}>
                                {String(index + 1).padStart(2, "0")}
                            </span>
                            <PilotName name={res.playerName} className="text-sm text-slate-200 truncate" />
                            <CountryFlag countryCode={res.country} className="text-sm" />
                            <div className="text-sm font-semibold text-slate-200 tabular-nums text-right">{res.points}</div>
                        </div>
                    </li>
                ))}
                {showMore && remaining.map((res, index) => (
                    <li
                        key={res.playerName}
                        className={`px-4 py-3 hover:bg-slate-600/20 transition-colors duration-150 ${(index + 25) % 2 === 0 ? "bg-slate-700/20" : ""}`}
                    >
                        <div className="grid grid-cols-[2.5rem_1fr_2rem_4rem] items-center gap-6">
                            <span className="text-right text-sm font-medium text-slate-400 tabular-nums">
                                {String(index + 26).padStart(2, "0")}
                            </span>
                            <PilotName name={res.playerName} className="text-sm text-slate-200 truncate" />
                            <CountryFlag countryCode={res.country} className="text-sm" />
                            <div className="text-sm font-semibold text-slate-200 tabular-nums text-right">{res.points}</div>
                        </div>
                    </li>
                ))}
            </ul>
            {remaining.length > 0 && !showMore && (
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

export default LeaderBoard;
