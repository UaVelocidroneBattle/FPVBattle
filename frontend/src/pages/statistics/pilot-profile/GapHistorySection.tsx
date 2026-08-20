import { PilotClassRatingModel, PilotRatingHistoryPoint } from '@/api/client';
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
    return history.filter(p => p.date != null && new Date(p.date) >= cutoff);
}

interface GapHistorySectionProps {
    classRatings: PilotClassRatingModel[];
}

function GapHistorySection({ classRatings }: GapHistorySectionProps) {
    const chartable = classRatings.filter(c => c.ratingHistory.length > 1);
    const [selectedCupId, setSelectedCupId] = useState((chartable[0] ?? classRatings[0]).cupId);
    const [range, setRange] = useState<Range>('All');

    const selectedClass = classRatings.find(c => c.cupId === selectedCupId) ?? classRatings[0];
    const filtered = filterByRange(selectedClass.ratingHistory, range);

    return (
        <div className="bg-slate-800 p-6 hidden sm:block">
            <div className="flex items-center justify-between mb-8 flex-wrap gap-4">
                <div className="flex items-center gap-4">
                    <h2 className="text-xl font-semibold text-white">Gap to Leader History</h2>
                    {classRatings.length > 1 && (
                        <div className="flex gap-1">
                            {classRatings.map(c => (
                                <button
                                    key={c.cupId}
                                    onClick={() => setSelectedCupId(c.cupId)}
                                    disabled={c.ratingHistory.length <= 1}
                                    className={`px-3 py-1 text-sm rounded transition-colors duration-150 ${
                                        selectedCupId === c.cupId
                                            ? 'bg-emerald-500 text-white'
                                            : 'bg-slate-700 text-slate-400 hover:bg-slate-600 hover:text-white'
                                    } disabled:opacity-30 disabled:cursor-not-allowed disabled:hover:bg-slate-700 disabled:hover:text-slate-400`}
                                >
                                    {c.className}
                                </button>
                            ))}
                        </div>
                    )}
                </div>
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
