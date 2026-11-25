import { useState } from "react";
import { useLocation } from "react-router-dom";
import { ChevronDown } from "lucide-react";
import ConfirmDialog from "@/components/ui/ConfirmDialog";
import { useCups } from "@/hooks/useCups";
import { inputClass } from "@/lib/formStyles";
import { getCompetitionStore } from "@/store/competitionStore";

/**
 * Lets a conductor pick a cup and swap its running track for a new one. Mount it only while it is
 * open, so the cup starts as the one on screen each time.
 */
function RestartTrackDialog({ onClose }: { onClose: () => void }) {
    const { cups, findCup, defaultCup } = useCups();
    const { pathname } = useLocation();
    const [cupId, setCupId] = useState(() => findCup(pathname.split("/")[1])?.id ?? defaultCup?.id ?? "");

    const restart = async () =>
        cupId ? getCompetitionStore(cupId).getState().restartTrack() : "Pick a cup to restart.";

    return (
        <ConfirmDialog
            open
            title="Restart the track?"
            description="The cup's running track is cancelled and a new one replaces it. Times already set on it earn no points, and the chats announce that you restarted it."
            confirmLabel="Restart track"
            onConfirm={restart}
            onClose={onClose}
        >
            <div>
                <label htmlFor="restart-cup" className="mb-1.5 block text-sm text-slate-400">
                    Cup
                </label>
                {/* The browser's own arrow ignores the padding and hugs the border, so it is drawn here instead */}
                <div className="relative">
                    <select
                        id="restart-cup"
                        value={cupId}
                        onChange={(event) => setCupId(event.target.value)}
                        className={`${inputClass} appearance-none pr-9`}
                    >
                        {cups.map((cup) => (
                            <option key={cup.id} value={cup.id}>
                                {cup.name}
                            </option>
                        ))}
                    </select>
                    <ChevronDown className="pointer-events-none absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
                </div>
            </div>
        </ConfirmDialog>
    );
}

export default RestartTrackDialog;
