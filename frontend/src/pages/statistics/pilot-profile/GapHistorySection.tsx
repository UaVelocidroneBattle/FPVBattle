import { PilotRatingHistoryPoint } from '@/api/client';
import { ChartContainer } from '@/components/ChartContainer';
import { lazy, useState } from 'react';

const GapHistoryChart = lazy(() => import('./GapHistoryChart'));

type Range = '1M' | '6M' | '1Y' | 'All';

const ranges: { label: string; value: Range }[] = [
    { label: '1M', value: '1M' },
    { label: '6M', value: '6M' },
    { label: '1Y', value: '1Y' },
    { label: 'All', value: 'All' },
];

function filterByRange(history: PilotRatingHistoryPoint[], range: Range): PilotRatingHistoryPoint[] {
    if (range === 'All') return history;
    const now = new Date();
    const cutoff = new Date(now);
    if (range === '1M') cutoff.setMonth(now.getMonth() - 1);
    if (range === '6M') cutoff.setMonth(now.getMonth() - 6);
    if (range === '1Y') cutoff.setFullYear(now.getFullYear() - 1);
    return history.filter(p => new Date(p.date) >= cutoff);
}

interface GapHistorySectionProps {
    history: PilotRatingHistoryPoint[];
}

function GapHistorySection({ history }: GapHistorySectionProps) {
    const [range, setRange] = useState<Range>('All');
    const filtered = filterByRange(history, range);

    return (
        <div className="bg-slate-800 p-6 hidden sm:block">
            <div className="flex items-center justify-between mb-8">
                <h2 className="text-xl font-semibold text-white">Gap to Leader History</h2>
                <div className="flex gap-1">
                    {ranges.map(r => (
                        <button
                            key={r.value}
                            onClick={() => setRange(r.value)}
                            className={`px-3 py-1 text-sm rounded transition-colors duration-150 ${
                                range === r.value
                                    ? 'bg-emerald-500 text-white'
                                    : 'bg-slate-700 text-slate-400 hover:bg-slate-600 hover:text-white'
                            }`}
                        >
                            {r.label}
                        </button>
                    ))}
                </div>
            </div>
            {filtered.length > 1 ? (
                <ChartContainer className="bg-none" height="280px" overflowVisible>
                    <GapHistoryChart history={filtered} />
                </ChartContainer>
            ) : (
                <div className="text-gray-500 text-center py-8">Not enough data for this period</div>
            )}
        </div>
    );
}

export default GapHistorySection;
