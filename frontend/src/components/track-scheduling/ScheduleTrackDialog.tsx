import { useState } from "react";
import { ChevronDown } from "lucide-react";
import type { SchedulableTrackModel, QueuedTrackModel, TrackSchedulingOverviewModel } from "@/api/client";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import { inputClass, primaryButtonClass, secondaryButtonClass } from "@/lib/formStyles";
import { useTrackSchedulingStore } from "@/store/trackSchedulingStore";
import TrackSuggestInput from "./TrackSuggestInput";
import { describeQueuePosition } from "./queuePosition";

const ANY_QUAD = "";

interface ScheduleTrackDialogProps {
    open: boolean;
    overview: TrackSchedulingOverviewModel;
    /** The cup of the page the dialog was opened from. */
    cupId: string;
    onClose: () => void;
}

interface ScheduleTrackFormProps {
    overview: TrackSchedulingOverviewModel;
    cupId: string;
    busy: boolean;
    onBusyChange: (busy: boolean) => void;
    onScheduled: (queuedTrack: QueuedTrackModel) => void;
    onClose: () => void;
}

function ScheduleTrackForm({ overview, cupId, busy, onBusyChange, onScheduled, onClose }: ScheduleTrackFormProps) {
    const [trackText, setTrackText] = useState("");
    const [track, setTrack] = useState<SchedulableTrackModel | null>(null);
    const [quadId, setQuadId] = useState(ANY_QUAD);
    const [error, setError] = useState<string | null>(null);

    const cup = overview.cups.find((candidate) => candidate.cupId === cupId);
    const quads = cup?.quads ?? [];
    const canSubmit = track !== null && !busy;

    const changeTrackText = (text: string) => {
        setTrackText(text);
        if (track && text !== track.name) setTrack(null);
    };

    const pickTrack = (picked: SchedulableTrackModel) => {
        setTrack(picked);
        setTrackText(picked.name);
        setError(null);
    };

    const submit = async () => {
        if (!track || busy) return;

        onBusyChange(true);
        setError(null);
        const result = await useTrackSchedulingStore
            .getState()
            .schedule(cupId, track.trackId, quadId === ANY_QUAD ? null : Number(quadId));
        onBusyChange(false);

        if (result.ok) onScheduled(result.queuedTrack);
        else setError(result.error);
    };

    return (
        <>
            <DialogHeader>
                <DialogTitle>Schedule a track{cup && ` for ${cup.name}`}</DialogTitle>
            </DialogHeader>

            <div>
                <label htmlFor="schedule-track" className="mb-1.5 block text-sm text-slate-400">
                    Track
                </label>
                <TrackSuggestInput
                    id="schedule-track"
                    cupId={cupId}
                    text={trackText}
                    onTextChange={changeTrackText}
                    onSelect={pickTrack}
                />
                {track && <p className="mt-1 text-xs text-slate-500">{track.sceneName}</p>}
            </div>

            {quads.length > 0 && (
                <div>
                    <label htmlFor="schedule-quad" className="mb-1.5 block text-sm text-slate-400">
                        Quad
                    </label>
                    {/* The browser's own arrow ignores the padding and hugs the border, so it is drawn here instead */}
                    <div className="relative">
                        <select
                            id="schedule-quad"
                            value={quadId}
                            onChange={(event) => setQuadId(event.target.value)}
                            className={`${inputClass} appearance-none pr-9`}
                        >
                            <option value={ANY_QUAD}>Any quad</option>
                            {quads.map((quad) => (
                                <option key={quad.id} value={quad.id}>
                                    {quad.name}
                                </option>
                            ))}
                        </select>
                        <ChevronDown className="pointer-events-none absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
                    </div>
                    <p className="mt-1 text-xs text-slate-500">A quad makes the day a Quad of the Day.</p>
                </div>
            )}

            <p className="text-sm text-slate-400">
                You have {overview.remaining} of {overview.monthlyLimit} tracks left this month.
            </p>

            {error && <p className="text-sm text-red-400">{error}</p>}

            <DialogFooter className="gap-2 sm:gap-0">
                <button onClick={onClose} disabled={busy} className={secondaryButtonClass}>
                    Cancel
                </button>
                <button onClick={submit} disabled={!canSubmit} className={primaryButtonClass}>
                    {busy ? "Scheduling…" : "Schedule"}
                </button>
            </DialogFooter>
        </>
    );
}

interface ScheduledConfirmationProps {
    queuedTrack: QueuedTrackModel;
    cupName: string;
    onClose: () => void;
}

function ScheduledConfirmation({ queuedTrack, cupName, onClose }: ScheduledConfirmationProps) {
    return (
        <>
            <DialogHeader>
                <DialogTitle>Track scheduled</DialogTitle>
                <DialogDescription className="text-slate-400">
                    {queuedTrack.trackName} is {describeQueuePosition(queuedTrack.queuePosition)} in {cupName}. You can
                    see or cancel it on your profile.
                </DialogDescription>
            </DialogHeader>
            <DialogFooter>
                <button onClick={onClose} className={primaryButtonClass} autoFocus>
                    Done
                </button>
            </DialogFooter>
        </>
    );
}

/**
 * Lets a patron schedule a track into the queue of the cup whose page they are on. The form lives inside
 * the dialog content, so it starts empty every time the dialog opens.
 */
function ScheduleTrackDialog({ open, overview, cupId, onClose }: ScheduleTrackDialogProps) {
    const [busy, setBusy] = useState(false);
    const [queuedTrack, setQueuedTrack] = useState<QueuedTrackModel | null>(null);

    const close = () => {
        if (busy) return;

        setQueuedTrack(null);
        onClose();
    };

    const cupName = overview.cups.find((cup) => cup.cupId === queuedTrack?.cupId)?.name ?? "";

    return (
        <Dialog open={open} onOpenChange={(isOpen) => !isOpen && close()}>
            {/* Only the confirmation has a description, so the form must not point to one */}
            <DialogContent className="border-slate-700 bg-slate-800 text-white" {...(!queuedTrack && { "aria-describedby": undefined })}>
                {queuedTrack ? (
                    <ScheduledConfirmation queuedTrack={queuedTrack} cupName={cupName} onClose={close} />
                ) : (
                    <ScheduleTrackForm
                        overview={overview}
                        cupId={cupId}
                        busy={busy}
                        onBusyChange={setBusy}
                        onScheduled={setQueuedTrack}
                        onClose={close}
                    />
                )}
            </DialogContent>
        </Dialog>
    );
}

export default ScheduleTrackDialog;
