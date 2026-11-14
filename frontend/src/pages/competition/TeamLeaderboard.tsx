import { useState } from "react";
import { ChevronRight } from "lucide-react";
import { TeamLeaderboardResultModel } from "@/api/client";
import PilotName from "@/components/PilotName";
import TeamName from "@/components/TeamName";
import CountryFlag from "@/components/ui/CountryFlag";
import { rankStyle } from "@/lib/rankStyle";
import { convertMsToSec } from "@/utils/utils";

interface TeamLeaderboardProps {
    results: TeamLeaderboardResultModel[];
    isEnded: boolean;
    myTeamId?: number;
}

/** Written out in full rather than composed, so Tailwind can see every class it has to generate. */
const COLUMNS = {
    running: "grid-cols-[1.5rem_1fr_2rem_4rem] md:grid-cols-[2.5rem_1fr_3rem_5rem]",
    ended: "grid-cols-[1.5rem_1fr_2rem_4rem_2rem] md:grid-cols-[2.5rem_1fr_3rem_5rem_3.5rem]",
};

const COLUMN_GAP = "gap-3 md:gap-6";

function ColumnHeaders({ cols, isEnded }: { cols: string; isEnded: boolean }) {
    return (
        <div className={`px-4 py-2 border-b border-slate-700 grid ${cols} ${COLUMN_GAP}`}>
            <div className="text-xs font-medium text-slate-500 text-right">#</div>
            <div className="text-xs font-medium text-slate-500">Team</div>
            <div className="text-xs font-medium text-slate-500 text-right" title="Pilots who flew">Pilots</div>
            <div className="text-xs font-medium text-slate-500 text-right" title="Average time of the team's pilots">Avg</div>
            {isEnded && <div className="text-xs font-medium text-slate-500 text-right">Pts</div>}
        </div>
    );
}

/**
 * Green for a time that pulls the team's average down, red for one that holds it back. Only while the
 * competition runs: a finished day's average is stored, but its pilots come from today's membership.
 */
function timeVersusAverageStyle(trackTime: number | undefined, averageTime: number | undefined): string {
    if (trackTime === undefined || averageTime === undefined || trackTime === averageTime) return "text-slate-400";
    return trackTime < averageTime ? "text-emerald-400" : "text-red-400";
}

/** The team's pilots, fastest first, shown when its row is opened. Indented to line up with the team name. */
function TeamPilots({ result, isEnded }: { result: TeamLeaderboardResultModel; isEnded: boolean }) {
    if (result.pilots.length === 0) {
        return <p className="px-4 py-2 pl-12 md:pl-20 text-xs text-slate-500">The team's pilots are no longer known</p>;
    }

    return (
        <ul className="bg-slate-900/40 border-t border-slate-700/60 py-1">
            {result.pilots.map((pilot, index) => (
                <li key={pilot.pilotName} className="px-4 py-1.5 pl-12 md:pl-20 grid grid-cols-[1.25rem_1.25rem_1fr_auto] items-center gap-4">
                    <span className="text-xs text-slate-500 tabular-nums text-right">{index + 1}</span>
                    <CountryFlag countryCode={pilot.country} className="text-sm" />
                    <PilotName name={pilot.pilotName} className="text-sm text-slate-300 truncate" />
                    <span className={`text-sm tabular-nums ${isEnded ? "text-slate-400" : timeVersusAverageStyle(pilot.trackTime, result.averageTime)}`}>
                        {convertMsToSec(pilot.trackTime)}
                    </span>
                </li>
            ))}
        </ul>
    );
}

function TeamRow({ result, index, cols, isEnded, isMine }: { result: TeamLeaderboardResultModel; index: number; cols: string; isEnded: boolean; isMine: boolean }) {
    const [open, setOpen] = useState(false);
    const rowStyle = isMine ? "bg-emerald-500/10 border-y border-emerald-400/40" : index % 2 === 0 ? "bg-slate-700/20" : "";

    return (
        <li className={rowStyle}>
            {/* The whole row toggles for the mouse; the chevron is the keyboard's way in, as a link can't sit inside a button */}
            <div
                onClick={() => setOpen(!open)}
                className={`px-4 py-3 grid ${cols} ${COLUMN_GAP} items-center hover:bg-slate-600/20 transition-colors duration-150 cursor-pointer`}
            >
                <span className={`text-right text-sm tabular-nums ${rankStyle(result.rank ?? 0)}`}>
                    {String(result.rank).padStart(2, "0")}
                </span>
                <span className="flex min-w-0 items-center gap-1.5 text-sm text-slate-200">
                    <button
                        type="button"
                        aria-expanded={open}
                        aria-label={open ? "Hide the team's pilots" : "Show the team's pilots"}
                        onClick={(e) => { e.stopPropagation(); setOpen(!open); }}
                        className="shrink-0 text-slate-500 hover:text-emerald-400"
                    >
                        <ChevronRight className={`h-3.5 w-3.5 transition-transform duration-300 ${open ? "rotate-90" : ""}`} />
                    </button>
                    <TeamName teamId={result.teamId} name={result.teamName} tag={result.teamTag} isDeleted={result.isDeleted} />
                </span>
                <span className="text-sm text-slate-400 tabular-nums text-right">{result.pilots.length || "—"}</span>
                <span className="text-sm font-semibold text-slate-200 tabular-nums text-right">{convertMsToSec(result.averageTime)}</span>
                {isEnded && <span className="text-sm font-semibold text-emerald-400 tabular-nums text-right">{result.points}</span>}
            </div>
            {/* Animating the row track from 0fr to 1fr slides the list open at its natural height.
                Visibility switches at the end of closing, so the hidden links can't be tabbed to. */}
            <div
                className={`grid transition-[grid-template-rows,visibility] duration-300 ease-out ${
                    open ? "grid-rows-[1fr] visible" : "grid-rows-[0fr] invisible"
                }`}
            >
                <div className="overflow-hidden">
                    <TeamPilots result={result} isEnded={isEnded} />
                </div>
            </div>
        </li>
    );
}

/** Today's teams, ranked by the average time of their pilots. A row opens to show who flew. */
function TeamLeaderboard({ results, isEnded, myTeamId }: TeamLeaderboardProps) {
    if (results.length === 0) {
        return (
            <div className="bg-slate-800/50 backdrop-blur-sm border-y sm:border border-slate-700 px-4 py-8 text-slate-400 text-sm">
                No team has enough pilots on the track yet
            </div>
        );
    }

    const cols = isEnded ? COLUMNS.ended : COLUMNS.running;

    return (
        <div className="bg-slate-800/50 backdrop-blur-sm border-y sm:border border-slate-700 overflow-hidden">
            <ColumnHeaders cols={cols} isEnded={isEnded} />
            <ul>
                {results.map((result, index) => (
                    <TeamRow key={result.teamId} result={result} index={index} cols={cols} isEnded={isEnded} isMine={result.teamId === myTeamId} />
                ))}
            </ul>
        </div>
    );
}

export default TeamLeaderboard;
