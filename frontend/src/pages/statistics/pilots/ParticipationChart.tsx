import { ResponsiveLine } from '@nivo/line';
import { ParticipationModel } from '@/api/client';
import { chartTheme, seriesColors } from '@/lib/chartTheme';
import { dayTickInterval, formatDay, parseDay } from './chartDates';

interface ParticipationChartProps {
    data: ParticipationModel;
}

/**
 * How many pilots flew each day, one line per cup.
 */
export default function ParticipationChart({ data }: ParticipationChartProps) {
    const series = data.series.map((cup) => ({
        id: cup.cupName,
        // A cup gets no entry on days it held no competition. Those days are dropped
        // rather than plotted as zero: nobody failed to turn up, there was nothing
        // to turn up to, and a dip to zero would read as the cup being deserted.
        data: cup.counts
            .map((count, index) => ({ x: parseDay(data.days[index]), y: count, day: data.days[index] }))
            .filter((point) => point.y !== null),
    }));

    return (
        <ResponsiveLine
            data={series}
            theme={chartTheme}
            colors={seriesColors}
            margin={{ top: 20, right: 30, bottom: 90, left: 60 }}
            curve="monotoneX"
            xScale={{ type: 'time', useUTC: false, precision: 'day' }}
            yScale={{ type: 'linear', min: 0, max: 'auto' }}
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
            enablePoints={false}
            enableGridX={false}
            enableTouchCrosshair={true}
            useMesh={true}
            tooltip={({ point }) => (
                <div className="border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-200 shadow-lg">
                    <div className="text-xs text-slate-400">
                        {formatDay((point.data as { day: string }).day)}
                    </div>
                    <div className="flex items-center gap-2">
                        <span
                            className="inline-block h-2 w-2 rounded-full"
                            style={{ background: point.seriesColor }}
                        />
                        <span className="font-semibold">{point.data.yFormatted}</span>
                        <span className="text-slate-400">{point.seriesId}</span>
                    </div>
                </div>
            )}
            legends={[
                {
                    anchor: 'bottom',
                    direction: 'row',
                    translateY: 80,
                    itemsSpacing: 0,
                    itemDirection: 'left-to-right',
                    itemWidth: 120,
                    itemHeight: 20,
                    itemOpacity: 0.75,
                    symbolSize: 12,
                    symbolShape: 'circle',
                },
            ]}
        />
    );
}
