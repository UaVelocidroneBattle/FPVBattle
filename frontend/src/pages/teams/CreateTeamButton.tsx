import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";

interface CreateTeamButtonProps {
    label: string;
    className: string;
    /** Why the user cannot create a team right now, or null when they can. */
    blockedReason: string | null;
    onClick: () => void;
}

/** A create button that stays visible when creating is not possible, and tells why on hover. */
function CreateTeamButton({ label, className, blockedReason, onClick }: CreateTeamButtonProps) {
    const button = (
        <button onClick={onClick} disabled={blockedReason !== null} className={className}>
            {label}
        </button>
    );

    if (blockedReason === null) return button;

    // A disabled button gets no pointer events, so the wrapper carries the hover
    return (
        <TooltipProvider delayDuration={150}>
            <Tooltip>
                <TooltipTrigger asChild>
                    <span tabIndex={0} className="inline-block">
                        {button}
                    </span>
                </TooltipTrigger>
                <TooltipContent className="border-slate-700 bg-slate-800 text-slate-300">{blockedReason}</TooltipContent>
            </Tooltip>
        </TooltipProvider>
    );
}

export default CreateTeamButton;
