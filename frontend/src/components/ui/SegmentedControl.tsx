export interface SegmentedControlOption<T> {
    label: string;
    value: T;
}

interface SegmentedControlProps<T> {
    options: SegmentedControlOption<T>[];
    value: T;
    onChange: (value: T) => void;
    ariaLabel: string;
}

/** A row of buttons of which exactly one is pressed. */
function SegmentedControl<T>({ options, value, onChange, ariaLabel }: SegmentedControlProps<T>) {
    return (
        <div role="group" aria-label={ariaLabel} className="flex border border-slate-600 text-xs">
            {options.map((option) => (
                <button
                    key={option.label}
                    type="button"
                    aria-pressed={value === option.value}
                    onClick={() => onChange(option.value)}
                    className={`px-2.5 py-1 transition-colors ${
                        value === option.value
                            ? "bg-emerald-500/15 text-emerald-400"
                            : "text-slate-400 hover:text-emerald-400"
                    }`}
                >
                    {option.label}
                </button>
            ))}
        </div>
    );
}

export default SegmentedControl;
