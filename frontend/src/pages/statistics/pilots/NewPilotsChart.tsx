import { ResponsiveBar } from '@nivo/bar';
import { NewPilotsCountModel } from '@/api/client';
import { chartTheme, seriesColors } from '@/lib/chartTheme';
import { formatMonthLong, formatMonthShort } from './chartDates';

interface NewPilotsChartProps {
    data: NewPilotsCountModel;
}

/**
 * How many pilots joined in each month.
 *
 * Bars rather than a line: arrivals are a count per month, not a level that exists
 * between two months, and a line would invite reading a value off a point where
 * there is nothing to read.
 */
export default function NewPilotsChart({ data }: NewPilotsChartProps) {
    const bars = data.months.map((month, index) => ({
        month,
        pilots: data.counts[index],
    }));

    return (
        <ResponsiveBar
            data={bars}
            keys={['pilots']}
            indexBy="month"
            theme={chartTheme}
            colors={[seriesColors[0]]}
            margin={{ top: 20, right: 30, bottom: 70, left: 60 }}
            padding={0.3}
            enableLabel={false}
            enableGridX={false}
            axisTop={null}
            axisRight={null}
            axisBottom={{
                tickSize: 5,
                tickPadding: 5,
                tickRotation: -45,
                format: formatMonthShort,
                tickValues: everyNth(data.months, labelStep(data.months.length)),
            }}
            axisLeft={{
                tickSize: 5,
                tickPadding: 5,
                legend: 'New pilots',
                legendOffset: -50,
                legendPosition: 'middle',
            }}
            tooltip={({ data: bar, value }) => (
                <div className="border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-200 shadow-lg">
                    <div className="text-xs text-slate-400">{formatMonthLong(bar.month)}</div>
                    <div className="font-semibold">
                        {value} new {value === 1 ? 'pilot' : 'pilots'}
                    </div>
                </div>
            )}
            role="img"
            ariaLabel="New pilots per month"
        />
    );
}

/**
 * Bars stay one per month however long the range is; only the labels thin out, so a
 * three-year view keeps its shape without its axis turning into a smear.
 */
function labelStep(months: number): number {
    if (months > 36) return 6;
    if (months > 18) return 3;

    return 1;
}

function everyNth(months: string[], step: number): string[] {
    return step === 1 ? months : months.filter((_, index) => index % step === 0);
}
