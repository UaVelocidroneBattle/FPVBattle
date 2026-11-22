import { useState } from "react";
import type { QueuedTrackModel, TrackSchedulingOverviewModel } from "@/api/client";
import ConfirmDialog from "@/components/ui/ConfirmDialog";
import { compactSecondaryButtonClass } from "@/lib/formStyles";
import { useTrackSchedulingStore } from "@/store/trackSchedulingStore";
import { describeQueuePosition } from "./queuePosition";

interface QueuedTrackRowProps {
    queuedTrack: QueuedTrackModel;
    cupName: string;
}

function QueuedTrackRow({ queuedTrack, cupName }: QueuedTrackRowProps) {
    const [confirming, setConfirming] = useState(false);

    // On success the row disappears with the refreshed overview; a failure stays in the dialog
    const cancel = async () => {
        const result = await useTrackSchedulingStore.getState().cancel(queuedTrack.id);
        return result.ok ? null : result.error ?? "Could not cancel the track. Please try again.";
    };

    return (
        <li className="flex items-start justify-between gap-4 py-3">
            <div className="min-w-0">
                <p className="truncate">
                    {queuedTrack.trackName} <span className="text-slate-500">· {queuedTrack.sceneName}</span>
                </p>
                <p className="text-sm text-slate-400">
                    {cupName} · {queuedTrack.quadName ?? "any quad"} · {describeQueuePosition(queuedTrack.queuePosition)}
                </p>
            </div>
            <button onClick={() => setConfirming(true)} className={compactSecondaryButtonClass}>
                Cancel
            </button>
            <ConfirmDialog
                open={confirming}
                title="Cancel this track?"
                description={`${queuedTrack.trackName} leaves the ${cupName} queue.`}
                confirmLabel="Cancel track"
                onConfirm={cancel}
                onClose={() => setConfirming(false)}
            />
        </li>
    );
}

/** The patron's tracks waiting in a queue, with their monthly quota. Cancelling a track frees its slot. */
function MyQueuedTracks({ overview }: { overview: TrackSchedulingOverviewModel }) {
    const cupName = (cupId: string) => overview.cups.find((cup) => cup.cupId === cupId)?.name ?? cupId;

    return (
        <>
            {overview.monthlyLimit > 0 && (
                <p className="text-sm text-slate-400">
                    {overview.remaining} of {overview.monthlyLimit} tracks left this month.
                </p>
            )}

            {overview.myTracks.length === 0 ? (
                <p className="mt-2 text-sm text-slate-500">
                    No tracks waiting. Use "Schedule a track" on the daily competition page to pick one.
                </p>
            ) : (
                <ul className="mt-2 divide-y divide-slate-700">
                    {overview.myTracks.map((queuedTrack) => (
                        <QueuedTrackRow key={queuedTrack.id} queuedTrack={queuedTrack} cupName={cupName(queuedTrack.cupId)} />
                    ))}
                </ul>
            )}
        </>
    );
}

export default MyQueuedTracks;
