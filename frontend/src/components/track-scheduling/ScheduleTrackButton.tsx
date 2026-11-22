import { useState } from "react";
import BlockableButton from "@/components/ui/BlockableButton";
import { useTrackSchedulingStore } from "@/store/trackSchedulingStore";
import ScheduleTrackDialog from "./ScheduleTrackDialog";

const buttonClass =
    "border border-slate-600 px-3 py-1 text-sm text-slate-300 whitespace-nowrap transition-colors hover:border-emerald-400 hover:text-emerald-400 disabled:pointer-events-none disabled:opacity-50";

/**
 * Opens the dialog for scheduling a track for the given cup. Everyone sees it: someone who can't schedule
 * gets it disabled, with the reason on hover or tap. The page loads the overview with useTrackSchedulingSync.
 */
function ScheduleTrackButton({ cupId }: { cupId: string }) {
    const overview = useTrackSchedulingStore((state) => state.overview);
    const [open, setOpen] = useState(false);

    if (!overview?.enabled) return null;

    return (
        <>
            <BlockableButton
                label="Schedule a track"
                className={buttonClass}
                blockedReason={overview.blockedReason ?? null}
                onClick={() => setOpen(true)}
            />
            <ScheduleTrackDialog open={open} overview={overview} cupId={cupId} onClose={() => setOpen(false)} />
        </>
    );
}

export default ScheduleTrackButton;
