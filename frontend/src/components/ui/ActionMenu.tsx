import { useState } from "react";
import { MoreHorizontal } from "lucide-react";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";

export interface ActionMenuItem {
    label: string;
    onSelect: () => void;
    danger?: boolean;
    disabled?: boolean;
    title?: string;
}

interface ActionMenuProps {
    label: string;
    items: ActionMenuItem[];
}

function ActionMenu({ label, items }: ActionMenuProps) {
    const [open, setOpen] = useState(false);

    return (
        <Popover open={open} onOpenChange={setOpen}>
            <PopoverTrigger asChild>
                <button
                    aria-label={label}
                    className="p-2 text-slate-400 transition-colors hover:text-emerald-400"
                >
                    <MoreHorizontal className="h-4 w-4" />
                </button>
            </PopoverTrigger>

            <PopoverContent align="end" className="w-48 border-slate-700 bg-slate-900 p-0 text-slate-200">
                {items.map((item) => (
                    <button
                        key={item.label}
                        disabled={item.disabled}
                        title={item.title}
                        onClick={() => {
                            setOpen(false);
                            item.onSelect();
                        }}
                        className={`block w-full px-4 py-2.5 text-left text-sm transition-colors hover:bg-slate-800 disabled:pointer-events-none disabled:opacity-50 ${
                            item.danger ? "text-red-400" : "text-slate-300 hover:text-emerald-400"
                        }`}
                    >
                        {item.label}
                    </button>
                ))}
            </PopoverContent>
        </Popover>
    );
}

export default ActionMenu;
