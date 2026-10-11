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
import { inputClass, primaryButtonClass, secondaryButtonClass } from "./teamStyles";

const NAME_MAX_LENGTH = 32;
const DESCRIPTION_MAX_LENGTH = 500;
const TAG_MIN_LENGTH = 2;
const TAG_MAX_LENGTH = 5;

/** Tags are upper-case letters and digits only, so anything else is dropped as the user types. */
function toTag(value: string): string {
    return value.toUpperCase().replace(/[^A-Z0-9]/g, "");
}

interface EditedTeam {
    id: number;
    name: string;
    tag?: string | null;
    description?: string | null;
}

interface TeamFormDialogProps {
    open: boolean;
    /** The team to edit. Without it the dialog creates a new team. */
    team?: EditedTeam;
    onClose: () => void;
}

interface TeamFormProps {
    team?: EditedTeam;
    busy: boolean;
    onBusyChange: (busy: boolean) => void;
    onClose: () => void;
}

function TeamForm({ team, busy, onBusyChange, onClose }: TeamFormProps) {
    const [name, setName] = useState(team?.name ?? "");
    const [tag, setTag] = useState(team?.tag ?? "");
    const [description, setDescription] = useState(team?.description ?? "");
    const [error, setError] = useState<string | null>(null);

    const canSubmit = name.trim().length > 0 && tag.length >= TAG_MIN_LENGTH && !busy;

    const submit = async () => {
        if (!canSubmit) return;

        const { createTeam, updateTeam } = useTeamsStore.getState();

        onBusyChange(true);
        setError(null);
        const result = team
            ? await updateTeam(team.id, name.trim(), tag, description.trim())
            : await createTeam(name.trim(), tag, description.trim());
        onBusyChange(false);

        if (result.ok) onClose();
        else setError(result.error ?? null);
    };

    return (
        <>
            <DialogHeader>
                <DialogTitle>{team ? "Edit team" : "Create a team"}</DialogTitle>
                <DialogDescription className="text-slate-400">
                    {team
                        ? "Change the name, tag and description of the team."
                        : "You'll be its owner and can invite other pilots right after."}
                </DialogDescription>
            </DialogHeader>

            <div>
                <label htmlFor="team-name" className="mb-1.5 flex justify-between text-sm text-slate-400">
                    <span>Team name</span>
                    <span>{name.length}/{NAME_MAX_LENGTH}</span>
                </label>
                <input
                    id="team-name"
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    onKeyDown={(e) => e.key === "Enter" && submit()}
                    maxLength={NAME_MAX_LENGTH}
                    className={inputClass}
                    autoFocus
                />
            </div>

            <div>
                <label htmlFor="team-tag" className="mb-1.5 flex justify-between text-sm text-slate-400">
                    <span>
                        Tag <span className="text-slate-500">({TAG_MIN_LENGTH}-{TAG_MAX_LENGTH} letters or digits)</span>
                    </span>
                    <span>{tag.length}/{TAG_MAX_LENGTH}</span>
                </label>
                <input
                    id="team-tag"
                    value={tag}
                    onChange={(e) => setTag(toTag(e.target.value))}
                    onKeyDown={(e) => e.key === "Enter" && submit()}
                    maxLength={TAG_MAX_LENGTH}
                    autoComplete="off"
                    className={`${inputClass} font-mono tracking-wider`}
                />
            </div>

            <div>
                <label htmlFor="team-description" className="mb-1.5 flex justify-between text-sm text-slate-400">
                    <span>
                        Description <span className="text-slate-500">(optional)</span>
                    </span>
                    <span>{description.length}/{DESCRIPTION_MAX_LENGTH}</span>
                </label>
                <textarea
                    id="team-description"
                    value={description}
                    onChange={(e) => setDescription(e.target.value)}
                    maxLength={DESCRIPTION_MAX_LENGTH}
                    rows={4}
                    className={`${inputClass} resize-y`}
                />
            </div>

            {error && <p className="text-sm text-red-400">{error}</p>}

            <DialogFooter className="gap-2 sm:gap-0">
                <button onClick={onClose} disabled={busy} className={secondaryButtonClass}>
                    Cancel
                </button>
                <button onClick={submit} disabled={!canSubmit} className={primaryButtonClass}>
                    {team ? (busy ? "Saving…" : "Save") : busy ? "Creating…" : "Create team"}
                </button>
            </DialogFooter>
        </>
    );
}

/**
 * Creates a team, or edits one when `team` is given. The form lives inside the dialog content,
 * so it starts from the team's current values every time the dialog opens.
 */
function TeamFormDialog({ open, team, onClose }: TeamFormDialogProps) {
    const [busy, setBusy] = useState(false);

    const close = () => {
        if (!busy) onClose();
    };

    return (
        <Dialog open={open} onOpenChange={(isOpen) => !isOpen && close()}>
            <DialogContent className="border-slate-700 bg-slate-800 text-white">
                <TeamForm team={team} busy={busy} onBusyChange={setBusy} onClose={close} />
            </DialogContent>
        </Dialog>
    );
}

export default TeamFormDialog;
