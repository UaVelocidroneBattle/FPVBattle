import { ResponsiveLine, Serie } from '@nivo/line'
import PilotsChartProps from './PilotChartProps';

const PilotsChartAbsolute = ({ pilots, results }: PilotsChartProps) => {

    if (pilots.length < 1 || results.length < 1 || results.some(r => r.length == 0)) return <></>;

    const fromDate = new Date();
    fromDate.setMonth(fromDate.getMonth() - 2); // Get date from 2 months ago


    const chartData: Serie[] = pilots.map((pilot, index) => ({
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


    return (
        <ResponsiveLine
            data={data}
            margin={{ top: 50, right: 110, bottom: 50, left: 60 }}
            areaOpacity={0.07}
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
            pointSize={10}
            pointColor={{ theme: 'background' }}
            pointBorderWidth={2}
            pointBorderColor={{ from: 'serieColor' }}
            pointLabel="data.yFormatted"
            pointLabelYOffset={-12}
            enableTouchCrosshair={true}
            useMesh={true}
            legends={[
                {
                    anchor: 'bottom-right',
                    direction: 'column',
                    justify: false,
                    translateX: 100,
                    translateY: 0,
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

export default PilotsChartAbsolute;
