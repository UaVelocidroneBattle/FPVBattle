"use client"

import * as React from "react"
import { Check, ChevronDown, ChevronsUpDown } from "lucide-react"

import { cn } from "@/lib/utils"
import { Button } from "@/components/ui/button"
import {
    Command,
    CommandEmpty,
    CommandGroup,
    CommandInput,
    CommandItem,
    CommandList,
} from "@/components/ui/command"
import {
    Popover,
    PopoverContent,
    PopoverTrigger,
} from "@/components/ui/popover"


/**
 * How the trigger is drawn: a bordered box, or bare text with a chevron for
 * tight spots such as a table header row.
 */
export type ComboBoxVariant = "outline" | "inline";

const triggers = {
    outline: {
        buttonVariant: "outline",
        className: "w-[200px] justify-between bg-slate-800/50 hover:bg-slate-700/50 text-slate-200 hover:text-slate-200 border-slate-700",
        Chevron: ChevronsUpDown,
        chevronClassName: "ml-2 h-4 w-4 shrink-0 opacity-50",
    },
    inline: {
        buttonVariant: "ghost",
        className: "h-auto w-auto gap-1 p-0 text-xs font-medium text-slate-200 hover:bg-transparent hover:text-emerald-400",
        Chevron: ChevronDown,
        chevronClassName: "h-3 w-3 shrink-0 opacity-60",
    },
} as const;

interface ComboBoxProps<TItem> {
    defaultCaption: string;
    items: TItem[];
    getLabel: (item: TItem) => string;
    getKey: (item: TItem) => string;
    onSelect: (item: TItem) => void;
    value: TItem | null;
    /** Richer rendering of an item, e.g. with a flag. Defaults to its label. */
    renderLabel?: (item: TItem) => React.ReactNode;
    variant?: ComboBoxVariant;
    /** Tweaks the trigger, e.g. to match the text size around it. */
    triggerClassName?: string;
}

const Combobox = <T,>({ items, value, defaultCaption, getKey, getLabel, onSelect, renderLabel, variant = "outline", triggerClassName }: ComboBoxProps<T>) => {
    const [open, setOpen] = React.useState(false)

    const render = renderLabel ?? getLabel;
    const caption = value ? render(value) : defaultCaption;
    const trigger = triggers[variant];

    return (
        <Popover open={open} onOpenChange={setOpen}>
            <PopoverTrigger asChild>
                <Button
                    variant={trigger.buttonVariant}
                    role="combobox"
                    aria-expanded={open}
                    className={cn(trigger.className, triggerClassName)}
                >
                    {caption}
                    <trigger.Chevron className={trigger.chevronClassName} />
                </Button>
            </PopoverTrigger>
            <PopoverContent align="end" className="w-[200px] p-0 bg-slate-800 border-slate-700">
                <Command className="bg-slate-800">
                    <CommandInput placeholder="Search ..." className="text-slate-200 placeholder:text-slate-500 border-slate-700" />
                    <CommandList className="scrollbar-slim">
                        <CommandEmpty className="text-slate-400 py-4 text-center text-sm">Nothing found</CommandEmpty>
                        <CommandGroup>
                            {items.map((item) => (
                                <CommandItem
                                    key={getKey(item)}
                                    value={getLabel(item)}
                                    onSelect={() => {
                                        onSelect(item);
                                        setOpen(false)
                                    }}
                                    className="text-slate-200 data-[selected=true]:bg-slate-700 data-[selected=true]:text-white"
                                >
                                    <Check
                                        className={cn(
                                            "mr-2 h-4 w-4 text-emerald-400",
                                            value !== null && getKey(value) === getKey(item) ? "opacity-100" : "opacity-0"
                                        )}
                                    />
                                    {render(item)}
                                </CommandItem>
                            ))}
                        </CommandGroup>
                    </CommandList>
                </Command>
            </PopoverContent>
        </Popover>
    )
}


export default Combobox;