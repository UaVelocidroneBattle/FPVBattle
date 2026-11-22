import { useEffect, useId, useState, type KeyboardEvent } from "react";
import type { SchedulableTrackModel } from "@/api/client";
import { inputClass } from "@/lib/formStyles";
import { useTrackSuggestions } from "./useTrackSuggestions";

interface TrackSuggestInputProps {
    id: string;
    cupId: string;
    text: string;
    onTextChange: (text: string) => void;
    onSelect: (track: SchedulableTrackModel) => void;
}

/**
 * A track name input that suggests the cup's tracks as you type. A track that can't be scheduled
 * stays in the list with its reason, but can't be picked.
 */
function TrackSuggestInput({ id, cupId, text, onTextChange, onSelect }: TrackSuggestInputProps) {
    const listId = useId();
    const [open, setOpen] = useState(false);
    const [highlighted, setHighlighted] = useState(-1);
    const suggestions = useTrackSuggestions(cupId, text, open);
    const listVisible = open && suggestions.length > 0;

    useEffect(() => setHighlighted(-1), [suggestions]);

    const choose = (track: SchedulableTrackModel) => {
        if (track.restriction) return;

        onSelect(track);
        setOpen(false);
    };

    const handleKeyDown = (event: KeyboardEvent<HTMLInputElement>) => {
        if (!listVisible) return;

        if (event.key === "ArrowDown") {
            event.preventDefault();
            setHighlighted((index) => (index + 1) % suggestions.length);
        } else if (event.key === "ArrowUp") {
            event.preventDefault();
            setHighlighted((index) => (index <= 0 ? suggestions.length - 1 : index - 1));
        } else if (event.key === "Enter" && highlighted >= 0) {
            event.preventDefault();
            choose(suggestions[highlighted]);
        } else if (event.key === "Escape") {
            setOpen(false);
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
                placeholder="Start typing a track name"
                value={text}
                onChange={(event) => {
                    onTextChange(event.target.value);
                    setOpen(true);
                }}
                onFocus={() => setOpen(true)}
                onKeyDown={handleKeyDown}
                onBlur={() => setOpen(false)}
                className={inputClass}
                autoFocus
            />

            {listVisible && (
                <ul
                    id={listId}
                    role="listbox"
                    className="absolute z-10 mt-1 max-h-72 w-full overflow-y-auto border border-slate-600 bg-slate-900 shadow-lg scrollbar-slim"
                >
                    {suggestions.map((track, index) => (
                        <li
                            key={track.trackId}
                            id={`${listId}-${index}`}
                            role="option"
                            aria-selected={index === highlighted}
                            aria-disabled={track.restriction ? true : undefined}
                            // Keeps the focus in the input, so the list is not closed before the click lands
                            onMouseDown={(event) => {
                                event.preventDefault();
                                choose(track);
                            }}
                            onMouseEnter={() => setHighlighted(index)}
                            className={`px-3 py-2 text-sm ${index === highlighted ? "bg-slate-700" : ""} ${
                                track.restriction ? "cursor-default text-slate-500" : "cursor-pointer text-slate-200"
                            }`}
                        >
                            <span className="block truncate">
                                {track.name} <span className="text-slate-500">· {track.sceneName}</span>
                            </span>
                            {track.restriction && <span className="block text-xs text-amber-400/80">{track.restriction}</span>}
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}

export default TrackSuggestInput;
