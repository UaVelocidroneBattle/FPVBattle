import { ResponsiveLine, LineSeries } from '@nivo/line'
import { PartialTheme } from '@nivo/theming';
import PilotsChartProps from './PilotChartProps';
import RelativeChartTooltip from './RelativeChartTooltip';

export interface RelativeChartDataPoint {
    x: Date;
    y: number | null;
    absoluteDifference?: number;
}


const PilotsChartRelative = ({ pilots, results }: PilotsChartProps) => {

    if (pilots.length < 2 || results.length < 2 || results.some(r => r.length == 0)) return <>
        <h2>No data</h2>
    </>;

    // pilots[0] is always the reference pilot

    const fromDate = new Date();
    fromDate.setMonth(fromDate.getMonth() - 2); // Get date from 2 months ago

    let chartData: LineSeries[] = pilots.map((pilot, index) => ({
        id: pilot!,
        data: results[index].map(r => ({ x: new Date(r.date), y: r.trackTime / 1000 })).filter(i => i.x >= fromDate)
    }));

    const dates: Record<number, number> = {};
    for (const pilotData of chartData) {
        for (const result of pilotData.data) {
            dates[(result.x as Date).valueOf()] = (dates[(result.x as Date).valueOf()] || 0) + 1;
        }
    }

    for (const pilotData of chartData) {
        pilotData.data = pilotData.data.filter(i => dates[(i.x as Date).valueOf()] == chartData.length);
    }

    if (chartData.some(r => r.data.length == 0)) return <>
        <h2>Insufficient data</h2>
    </>

    const t = [];

    for (let pilotIndex = 0; pilotIndex < chartData.length; pilotIndex++) {
        if (pilotIndex == 0) continue; // Skip reference pilot (pilots[0])
        const pilotData = chartData[pilotIndex];

        const pilotDataPositive = {
            id: `Faster`,
            data: [] as RelativeChartDataPoint[]
        };

        const pilotDataNegative = {
            id: `Slower`,
            data: [] as RelativeChartDataPoint[]
        };

        let lastValue: { x: Date, y: number } | null = null;

        for (let i = 0; i < pilotData.data.length; i++) {
            const v = pilotData.data[i];
            const absoluteDifference = Math.abs((v.y as number) - (chartData[0].data[i].y as number));
            const y = ((v.y as number) - (chartData[0].data[i].y as number)) / (chartData[0].data[i].y as number) * 100;
            v.y = y;

            if (lastValue && (lastValue.y < 0 && y > 0 || lastValue.y > 0 && y < 0)) {
                //add intermediate point        
                const md = new Date(((v.x as Date).valueOf() + lastValue.x.valueOf()) / 2);
                pilotDataPositive.data.push({ x: md, y: 0, absoluteDifference });
                pilotDataNegative.data.push({ x: md, y: 0, absoluteDifference });
            }

            // Calculate absolute time difference using original times

            pilotDataPositive.data.push({ x: v.x as Date, y: y >= 0 ? y : null, absoluteDifference });
            pilotDataNegative.data.push({ x: v.x as Date, y: y <= 0 ? y : null, absoluteDifference });

            lastValue = v as { x: Date, y: number };
        }

        t.push(pilotDataPositive);
        t.push(pilotDataNegative);
    }

    chartData = t;

    // pilots[0] is reference, pilots[1] is compared pilot

    const theme: PartialTheme = {
        text: { fill: 'rgba(203, 213, 225, 0.5)' },
        crosshair: {
            line: {
                stroke: 'rgba(203, 213, 225, 0.5)',
                strokeWidth: 1
            }
        },
        grid: {
            line: {
                stroke: '#94a3b8',
                strokeWidth: 1,
                strokeOpacity: 0.2
            }
        },
        axis: {
            domain: { line: { stroke: 'rgba(203, 213, 225, 0.5)' } },
            ticks: {
                line: { stroke: 'rgba(203, 213, 225, 0.5)' },
                text: { fill: 'rgba(203, 213, 225, 0.5)' },
            },
            legend: { text: { fill: 'rgba(203, 213, 225, 0.5)' } },
        },
        legends: { text: { fill: 'rgba(203, 213, 225, 0.5)' } },
    };

    return (
        <ResponsiveLine
            data={chartData}
            margin={{ top: 20, right: 20, bottom: 100, left: 50 }}
            theme={theme}
            areaOpacity={0.3}
            colors={[
                'rgb(97, 205, 187)',
                'rgb(244, 117, 96)'
            ]}
            enableArea
            curve="monotoneX"
            tooltip={({ point }) => (
                <RelativeChartTooltip point={{ data: point.data as RelativeChartDataPoint }} pilots={pilots!} />
            )}
            xScale={{
                format: '%Y-%m-%d',
                precision: 'day',
                type: 'time',
                useUTC: false
            }}
            yScale={{
                type: 'linear',
                min: 'auto',
                max: 'auto',
                stacked: false,
                reverse: false
            }}
            yFormat=" >-.2f"
            xFormat="time:%Y-%m-%d"
            axisTop={null}
            axisRight={null}
            axisBottom={{
                tickSize: 5,
                tickPadding: 5,
                tickRotation: -45,
                legend: 'Date',
                legendPosition: 'middle',
                truncateTickAt: 0,
                format: '%b %d',
                legendOffset: -12,
                tickValues: 'every 2 days'
            }}
            axisLeft={{
                tickSize: 5,
                tickPadding: 5,
                tickRotation: 0,
                legend: 'Difference, %',
                legendOffset: -40,
                legendPosition: 'middle',
                truncateTickAt: 0
            }}
            enablePoints={true}
            pointSize={6}
            pointColor={{ from: 'seriesColor' }}
            pointBorderWidth={6}
            pointBorderColor={{ from: 'seriesColor' }}
            pointLabel="data.yFormatted"
            pointLabelYOffset={-12}
            enableTouchCrosshair={true}
            useMesh={true}
            legends={[
                {
                    anchor: 'bottom',
                    direction: 'row',
                    justify: false,
                    translateX: 0,
                    translateY: 70,
                    itemsSpacing: 0,
                    itemDirection: 'left-to-right',
                    itemWidth: 80,
                    itemHeight: 20,
                    itemOpacity: 0.75,
                    symbolSize: 12,
                    symbolShape: 'circle',
                    symbolBorderColor: 'rgba(0, 0, 0, .5)',
                    effects: [
                        {
                            on: 'hover',
                            style: {
                                itemBackground: 'rgba(0, 0, 0, .03)',
                                itemOpacity: 1
                            }
                        }
                    ]
                }
            ]}
        />
    );
}

export default PilotsChartRelative;
