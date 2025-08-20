import { useState } from "react";
import { SeasonResultModel } from "../api/client";
import { ChevronDown } from "lucide-react";
import { Spinner } from "@/components/ui/spinner.tsx";

interface LeaderBoardProps {
    leaderBoard: SeasonResultModel[];
}

interface LeaderRowProps {
    res: SeasonResultModel;
    index: number;
}

const LeaderRow: React.FC<LeaderRowProps> = ({ res, index }) => {
    const isTopTen = index < 10;

    const liClasses = isTopTen
        ? "px-6 py-4 hover:bg-slate-700/30 transition-colors duration-150"
        : "px-6 py-2 hover:bg-slate-700/20 transition-colors duration-150";

    const spanClasses = isTopTen
        ? "font-bold text-slate-400 mr-4 w-8 text-right text-2xl tabular-nums"
        : "font-medium text-slate-300 mr-2 w-6 text-right tabular-nums";

    const nameClasses = isTopTen
        ? "text-sm font-medium text-slate-200"
        : "text-sm text-slate-400";

    const pointsClasses = isTopTen
        ? "text-lg text-slate-300 font-semibold tabular-nums"
        : "text-sm text-slate-300 font-semibold tabular-nums";

    return (
        <li className={liClasses}>
            <div className="flex items-center justify-between">
                <div className="flex items-center">
                    <span className={spanClasses}>
                        {isTopTen ? String(index + 1).padStart(2, "0") : index + 1}
                    </span>
                    <p className={nameClasses}>{res.playerName}</p>
                </div>
                <div className={pointsClasses}>{res.points}</div>
            </div>
        </li>
    );
};

const LeaderBoard: React.FC<LeaderBoardProps> = ({ leaderBoard }) => {
    const [showMore, setShowMore] = useState(false);

    if (!leaderBoard) return <Spinner/>;
    if (!leaderBoard.length) return <div className="p-4 text-slate-400 text-center">no data</div>;

    const first25 = leaderBoard.slice(0, 25);
    const remaining = leaderBoard.slice(25);

    return (
        <div className="overflow-hidden">
            <div className="px-6 py-4 border-b border-slate-700/50 grid grid-cols-2 gap-4">
                <div className="text-sm font-medium text-emerald-400">Pilot</div>
                <div className="text-sm font-medium text-emerald-400 text-right">Points</div>
            </div>
            <ul className="divide-y divide-slate-700/50">
                {first25.map((res, index) => (
                    <LeaderRow key={res.playerName} res={res} index={index} />
                ))}
                {showMore &&
                    remaining.map((res, index) => (
                        <LeaderRow key={res.playerName} res={res} index={index + 25} />
                    ))}
            </ul>
            {remaining.length > 0 && !showMore && (
                <div className="flex justify-center mt-4">
                    <button
                        className="flex items-center text-emerald-400 hover:text-emerald-600 transition-colors font-medium"
                        onClick={() => setShowMore(true)}
                    >
                        Load more <ChevronDown className="ml-2 h-4 w-4" />
                    </button>
                </div>
            )}
        </div>
    );
};

export default LeaderBoard;
