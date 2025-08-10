"use client"

import * as React from "react"
import { Check, ChevronsUpDown } from "lucide-react"

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


interface ComboBoxProps<TItem> {
    defaultCaption: string;
    items: TItem[];
    getLabel: (item: TItem) => string;
    getKey: (item: TItem) => string;
    onSelect: (item: TItem) => void;
    value: TItem | null;
    leadingIcon?: React.ReactNode;
    getSelectionIcon?: (item: TItem, isSelected: boolean) => React.ReactNode;
}

const Combobox = <T,>({ items, value, defaultCaption, getKey, getLabel, onSelect, leadingIcon, getSelectionIcon }: ComboBoxProps<T>) => {
    const [open, setOpen] = React.useState(false)

    const caption = value ? getLabel(value) : defaultCaption;

    return (
        <Popover open={open} onOpenChange={setOpen}>
            <PopoverTrigger asChild>
                <Button
                    variant="outline"
                    role="combobox"
                    aria-expanded={open}
                    className="w-[200px] justify-between bg-slate-800/50 hover:bg-slate-700/50 text-slate-200 hover:text-slate-200 border-slate-700"
                >
                    <div className="flex items-center gap-2">
                        {leadingIcon}
                        <span>{caption}</span>
                    </div>
                    <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                </Button>
            </PopoverTrigger>
            <PopoverContent className="w-[200px] p-0">
                <Command>
                    <CommandInput placeholder="Search ..."/>
                    <CommandList>
                        <CommandEmpty>Nothing found</CommandEmpty>
                        <CommandGroup>
                            {items.map((item) => {
                                const isSelected = value !== null && getLabel(value) === getLabel(item);
                                return (
                                    <CommandItem
                                        key={getKey(item)}
                                        value={getLabel(item)}
                                        onSelect={() => {
                                            onSelect(item);
                                            setOpen(false)
                                        }}
                                    >
                                        <div className="mr-2 h-4 w-4 flex items-center justify-center">
                                            {getSelectionIcon ? (
                                                getSelectionIcon(item, isSelected)
                                            ) : (
                                                <Check
                                                    className={cn(
                                                        "h-4 w-4",
                                                        isSelected ? "opacity-100" : "opacity-0"
                                                    )}
                                                />
                                            )}
                                        </div>
                                        {getLabel(item)}
                                    </CommandItem>
                                );
                            })}
                        </CommandGroup>
                    </CommandList>
                </Command>
            </PopoverContent>
        </Popover>
    )
}


export default Combobox;