import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { deleteApiProfileAccount } from "@/api/client";
import { useAuthStore } from "@/store/authStore";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";

/**
 * The "Danger zone" of the profile page: permanently deletes the account
 * after an explicit confirmation, then signs the user out.
 */
function DeleteAccountSection() {
    const navigate = useNavigate();
    const [confirming, setConfirming] = useState(false);
    const [deleting, setDeleting] = useState(false);
    const [failed, setFailed] = useState(false);

    const deleteAccount = async () => {
        if (deleting) return;

        setDeleting(true);
        setFailed(false);
        try {
            const response = await deleteApiProfileAccount();
            if (!response.response.ok) throw new Error("Deletion was rejected");

            await useAuthStore.getState().logout();
            navigate("/");
        } catch {
            setFailed(true);
            setDeleting(false);
        }
    };

    return (
        <div>
            <p className="text-sm text-slate-400">
                Deleting your account removes your personal data. Your pilot, race history and
                statistics stay untouched. This cannot be undone.
            </p>

            <button
                onClick={() => setConfirming(true)}
                className="mt-4 border border-red-500/70 px-4 py-2 text-sm text-red-400 transition-colors hover:bg-red-500/10"
            >
                Delete account
            </button>

            <Dialog open={confirming} onOpenChange={(open) => !deleting && setConfirming(open)}>
                <DialogContent className="border-slate-700 bg-slate-800 text-white">
                    <DialogHeader>
                        <DialogTitle>Delete your account?</DialogTitle>
                        <DialogDescription className="text-slate-400">
                            Your account and sign-in data will be permanently deleted. Your pilot and
                            race results are community competition history and will remain. You can
                            sign up again later, but you will need to re-verify your pilot.
                        </DialogDescription>
                    </DialogHeader>

                    {failed && (
                        <p className="text-sm text-red-400">
                            Could not delete the account right now. Please try again.
                        </p>
                    )}

                    <DialogFooter>
                        <button
                            onClick={() => setConfirming(false)}
                            disabled={deleting}
                            className="border border-slate-600 px-4 py-2 text-sm transition-colors hover:border-emerald-400 hover:text-emerald-400 disabled:opacity-50"
                        >
                            Cancel
                        </button>
                        <button
                            onClick={deleteAccount}
                            disabled={deleting}
                            className="border border-red-500 px-4 py-2 text-sm text-red-400 transition-colors hover:bg-red-500/10 disabled:opacity-50"
                        >
                            {deleting ? "Deleting…" : "Delete forever"}
                        </button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>
        </div>
    );
}

export default DeleteAccountSection;
