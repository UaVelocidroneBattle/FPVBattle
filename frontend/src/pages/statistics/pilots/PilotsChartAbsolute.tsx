import { ResponsiveLine, LineSeries } from '@nivo/line'
import PilotsChartProps from './PilotChartProps';

const PilotsChartAbsolute = ({ pilots, results }: PilotsChartProps) => {

    if (pilots.length < 1 || results.length < 1 || results.some(r => r.length == 0)) return <></>;

    const fromDate = new Date();
    fromDate.setMonth(fromDate.getMonth() - 2); // Get date from 2 months ago


    const chartData: LineSeries[] = pilots.map((pilot, index) => ({
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

    const data = chartData.filter(d => d.data.length > 0);

    if (data.length == 0) return <>
        <h2>No data</h2>
    </>

    const theme = {
        text: { fill: 'rgba(203, 213, 225, 0.5)' },
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
            data={data}
            theme={theme}
            margin={{ top: 20, right: 20, bottom: 100, left: 50 }}
            areaOpacity={0.3}
            colors={[
                'rgb(97, 205, 187)',
                'rgb(244, 117, 96)'
            ]}
            curve="monotoneX"
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
                //legendOffset: 36,
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
                legend: 'Time (seconds)',
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
                    itemWidth: 150,
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

export default PilotsChartAbsolute;
