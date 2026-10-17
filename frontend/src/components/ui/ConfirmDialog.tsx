import { useState } from "react";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";

interface ConfirmDialogProps {
    open: boolean;
    title: string;
    description: string;
    confirmLabel: string;
    /** Resolves to an error message to show in the dialog, or null when the action succeeded. */
    onConfirm: () => Promise<string | null>;
    onClose: () => void;
}

function ConfirmDialog({ open, title, description, confirmLabel, onConfirm, onClose }: ConfirmDialogProps) {
    const [busy, setBusy] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const close = () => {
        if (busy) return;
        setError(null);
        onClose();
    };

    const confirm = async () => {
        setBusy(true);
        setError(null);
        const failure = await onConfirm();
        setBusy(false);

        if (failure) setError(failure);
        else onClose();
    };

    return (
        <Dialog open={open} onOpenChange={(isOpen) => !isOpen && close()}>
            <DialogContent className="border-slate-700 bg-slate-800 text-white">
                <DialogHeader>
                    <DialogTitle>{title}</DialogTitle>
                    <DialogDescription className="text-slate-400">{description}</DialogDescription>
                </DialogHeader>

                {error && <p className="text-sm text-red-400">{error}</p>}

                <DialogFooter className="gap-2 sm:gap-0">
                    <button
                        onClick={close}
                        disabled={busy}
                        className="border border-slate-600 px-4 py-2 text-sm transition-colors hover:border-emerald-400 hover:text-emerald-400 disabled:opacity-50"
                    >
                        Cancel
                    </button>
                    <button
                        onClick={confirm}
                        disabled={busy}
                        className="border border-red-500 px-4 py-2 text-sm text-red-400 transition-colors hover:bg-red-500/10 disabled:opacity-50"
                    >
                        {busy ? "Please wait…" : confirmLabel}
                    </button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}

export default ConfirmDialog;
