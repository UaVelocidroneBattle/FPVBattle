import { RangeOption } from '@/store/rangedChartStore';

interface RangeSelectorProps {
    ranges: readonly RangeOption[];
    selectedKey: string;
    onSelect: (key: string) => void;
    /** Names the group for screen readers, e.g. "Total pilots time range". */
    label: string;
}

/**
 * Segmented control for putting a chart on a different time scale.
 */
export function RangeSelector({ ranges, selectedKey, onSelect, label }: RangeSelectorProps) {
    return (
        <div role="group" aria-label={label} className="flex gap-1 border border-slate-700 bg-slate-900/40 p-1">
            {ranges.map((range) => {
                const selected = range.key === selectedKey;

                return (
                    <button
                        key={range.key}
                        type="button"
                        aria-pressed={selected}
                        onClick={() => onSelect(range.key)}
                        className={`px-3 py-1 text-xs whitespace-nowrap transition-colors ${selected
                            ? 'bg-slate-700 text-slate-100'
                            : 'text-slate-400 hover:bg-slate-800 hover:text-slate-200'
                            }`}
                    >
                        {range.label}
                    </button>
                );
            })}
        </div>
    );
}
