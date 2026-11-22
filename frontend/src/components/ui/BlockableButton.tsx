import { useState } from "react";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";

interface BlockableButtonProps {
    label: string;
    className: string;
    /** Why the action is not possible right now, or null when it is. */
    blockedReason: string | null;
    onClick: () => void;
}

/** A button that stays visible when its action is not possible, and tells why on hover or tap. */
function BlockableButton({ label, className, blockedReason, onClick }: BlockableButtonProps) {
    const [reasonShown, setReasonShown] = useState(false);

    const button = (
        <button onClick={onClick} disabled={blockedReason !== null} className={className}>
            {label}
        </button>
    );

    if (blockedReason === null) return button;

    // A disabled button gets no pointer events, so the wrapper carries the hover. Touch screens have no hover,
    // so a tap on the wrapper opens the reason too, and a tap anywhere else closes it.
    return (
        <TooltipProvider delayDuration={150}>
            <Tooltip open={reasonShown} onOpenChange={setReasonShown}>
                <TooltipTrigger asChild>
                    <span tabIndex={0} className="inline-block" onClick={() => setReasonShown(true)}>
                        {button}
                    </span>
                </TooltipTrigger>
                <TooltipContent className="border-slate-700 bg-slate-800 text-slate-300">{blockedReason}</TooltipContent>
            </Tooltip>
        </TooltipProvider>
    );
}

export default BlockableButton;
