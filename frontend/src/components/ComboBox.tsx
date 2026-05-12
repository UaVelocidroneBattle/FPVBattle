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
}

const Combobox = <T,>({ items, value, defaultCaption, getKey, getLabel, onSelect }: ComboBoxProps<T>) => {
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
                    {caption}
                    <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                </Button>
            </PopoverTrigger>
            <PopoverContent className="w-[200px] p-0 bg-slate-800 border-slate-700">
                <Command className="bg-slate-800">
                    <CommandInput placeholder="Search ..." className="text-slate-200 placeholder:text-slate-500 border-slate-700" />
                    <CommandList className="[&::-webkit-scrollbar]:w-1.5 [&::-webkit-scrollbar-track]:bg-slate-800 [&::-webkit-scrollbar-thumb]:bg-slate-600 [&::-webkit-scrollbar-thumb]:rounded-full">
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
                                            value === getLabel(item) ? "opacity-100" : "opacity-0"
                                        )}
                                    />
                                    {getLabel(item)}
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