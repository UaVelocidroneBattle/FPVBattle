import { ResponsiveLine } from '@nivo/line';
import { PilotsCountModel } from '@/api/client';
import { chartTheme, seriesColors } from '@/lib/chartTheme';
import { dayTickInterval, formatDay, parseDay } from './chartDates';

interface PilotsCountChartProps {
    data: PilotsCountModel;
}

/**
 * The size of the pilot community over time.
 *
 * A plain line rather than an area: the count never starts at zero on a windowed
 * range, and a filled shape resting on a cut-off axis would read as far more growth
 * than there is. The line is free to use the axis its data needs.
 */
export default function PilotsCountChart({ data }: PilotsCountChartProps) {
    const points = data.days.map((day, index) => ({
        x: parseDay(day),
        y: data.totals[index],
    }));

    return (
        <ResponsiveLine
            data={[{ id: 'Pilots', data: points }]}
            theme={chartTheme}
            colors={[seriesColors[0]]}
            margin={{ top: 20, right: 30, bottom: 60, left: 60 }}
            curve="monotoneX"
            xScale={{ type: 'time', useUTC: false, precision: 'day' }}
            yScale={{ type: 'linear', min: 'auto', max: 'auto' }}
            axisTop={null}
            axisRight={null}
            axisBottom={{
                tickSize: 5,
                tickPadding: 5,
                tickRotation: -45,
                format: '%b %Y',
                tickValues: dayTickInterval(data.days.length),
            }}
            axisLeft={{
                tickSize: 5,
                tickPadding: 5,
                legend: 'Pilots',
                legendOffset: -50,
                legendPosition: 'middle',
            }}
            enableArea={false}
            enablePoints={false}
            enableGridX={false}
            enableTouchCrosshair={true}
            useMesh={true}
            tooltip={({ point }) => (
                <div className="border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-200 shadow-lg">
                    <div className="text-xs text-slate-400">
                        {formatDay(data.days[point.indexInSeries])}
                    </div>
                    <div className="font-semibold">{point.data.yFormatted} pilots</div>
                </div>
            )}
        />
    );
}
