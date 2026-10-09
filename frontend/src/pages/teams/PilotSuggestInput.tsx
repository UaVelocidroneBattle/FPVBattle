import { useEffect, useId, useState, type KeyboardEvent } from "react";
import CountryFlag from "@/components/ui/CountryFlag";
import { inputClass } from "./teamStyles";
import { usePilotSuggestions } from "./usePilotSuggestions";

interface PilotSuggestInputProps {
    id: string;
    teamId: number;
    value: string;
    maxLength: number;
    onChange: (value: string) => void;
    onSubmit: () => void;
}

function PilotSuggestInput({ id, teamId, value, maxLength, onChange, onSubmit }: PilotSuggestInputProps) {
    const listId = useId();
    const [open, setOpen] = useState(false);
    const [highlighted, setHighlighted] = useState(-1);
    const suggestions = usePilotSuggestions(teamId, value, open);
    const listVisible = open && suggestions.length > 0;

    useEffect(() => setHighlighted(-1), [suggestions]);

    const choose = (name: string) => {
        onChange(name);
        setOpen(false);
    };

    const handleKeyDown = (event: KeyboardEvent<HTMLInputElement>) => {
        if (listVisible && event.key === "ArrowDown") {
            event.preventDefault();
            setHighlighted((index) => (index + 1) % suggestions.length);
        } else if (listVisible && event.key === "ArrowUp") {
            event.preventDefault();
            setHighlighted((index) => (index <= 0 ? suggestions.length - 1 : index - 1));
        } else if (event.key === "Enter") {
            if (listVisible && highlighted >= 0) {
                event.preventDefault();
                choose(suggestions[highlighted].name);
            } else {
                onSubmit();
            }
        }
    };

    return (
        <div className="relative">
            <input
                id={id}
                role="combobox"
                aria-expanded={listVisible}
                aria-controls={listId}
                aria-autocomplete="list"
                aria-activedescendant={highlighted >= 0 ? `${listId}-${highlighted}` : undefined}
                autoComplete="off"
                value={value}
                maxLength={maxLength}
                onChange={(event) => {
                    onChange(event.target.value);
                    setOpen(true);
                }}
                onKeyDown={handleKeyDown}
                onBlur={() => setOpen(false)}
                className={inputClass}
                autoFocus
            />

            {listVisible && (
                <ul
                    id={listId}
                    role="listbox"
                    className="absolute z-10 mt-1 max-h-60 w-full overflow-y-auto border border-slate-600 bg-slate-900 shadow-lg scrollbar-slim"
                >
                    {suggestions.map((suggestion, index) => (
                        <li
                            key={suggestion.name}
                            id={`${listId}-${index}`}
                            role="option"
                            aria-selected={index === highlighted}
                            // Keeps the focus in the input, so the list is not closed before the click lands
                            onMouseDown={(event) => {
                                event.preventDefault();
                                choose(suggestion.name);
                            }}
                            onMouseEnter={() => setHighlighted(index)}
                            className={`flex cursor-pointer items-center gap-3 px-3 py-2 text-sm text-slate-200 ${
                                index === highlighted ? "bg-slate-700" : ""
                            }`}
                        >
                            <span className="min-w-0 flex-1 truncate">{suggestion.name}</span>
                            {suggestion.country && <CountryFlag countryCode={suggestion.country} />}
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}

export default PilotSuggestInput;
