import { useState } from "react";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import { useTeamsStore } from "@/store/teamsStore";
import PilotSuggestInput from "./PilotSuggestInput";
import { primaryButtonClass, secondaryButtonClass } from "./teamStyles";

const PILOT_NAME_MAX_LENGTH = 128;

interface InviteDialogProps {
    open: boolean;
    teamId: number;
    onClose: () => void;
}

function InviteDialog({ open, teamId, onClose }: InviteDialogProps) {
    const [pilotName, setPilotName] = useState("");
    const [busy, setBusy] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const canSubmit = pilotName.trim().length > 0 && !busy;

    const close = () => {
        if (busy) return;
        setPilotName("");
        setError(null);
        onClose();
    };

    const submit = async () => {
        if (!canSubmit) return;

        setBusy(true);
        setError(null);
        const result = await useTeamsStore.getState().inviteToTeam(teamId, pilotName.trim());
        setBusy(false);

        if (result.ok) close();
        else setError(result.error ?? null);
    };

    return (
        <Dialog open={open} onOpenChange={(isOpen) => !isOpen && close()}>
            <DialogContent className="border-slate-700 bg-slate-800 text-white">
                <DialogHeader>
                    <DialogTitle>Invite a pilot</DialogTitle>
                    <DialogDescription className="text-slate-400">
                        The pilot must not be in another team.
                    </DialogDescription>
                </DialogHeader>

                <div>
                    <label htmlFor="invite-pilot-name" className="mb-1.5 block text-sm text-slate-400">
                        Pilot name <span className="text-slate-500">(case sensitive)</span>
                    </label>
                    <PilotSuggestInput
                        id="invite-pilot-name"
                        teamId={teamId}
                        value={pilotName}
                        maxLength={PILOT_NAME_MAX_LENGTH}
                        onChange={setPilotName}
                        onSubmit={submit}
                    />
                </div>

                {error && <p className="text-sm text-red-400">{error}</p>}

                <DialogFooter className="gap-2 sm:gap-0">
                    <button onClick={close} disabled={busy} className={secondaryButtonClass}>
                        Cancel
                    </button>
                    <button onClick={submit} disabled={!canSubmit} className={primaryButtonClass}>
                        {busy ? "Sending…" : "Send invite"}
                    </button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}

export default InviteDialog;
