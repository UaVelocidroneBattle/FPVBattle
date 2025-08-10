import { ResponsiveLine, LineSeries } from '@nivo/line'
import PilotsChartProps from './PilotChartProps';


const PilotsChartRelative = ({ pilots, results }: PilotsChartProps) => {

    if (pilots.length < 2 || results.length < 2 || results.some(r => r.length == 0)) return <>
        <h2>No data</h2>
    </>;

    const referencePilot = 0;

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
        if (pilotIndex == referencePilot) continue;
        const pilotData = chartData[pilotIndex];

        const pilotDataPositive = {
            id: `Faster`,
            data: [] as { x: Date, y: number | null }[]
        };

        const pilotDataNegative = {
            id: `Slower`,
            data: [] as { x: Date, y: number | null }[]
        };

        let lastValue: { x: Date, y: number } | null = null;

        for (let i = 0; i < pilotData.data.length; i++) {
            const v = pilotData.data[i];
            const y = ((chartData[referencePilot].data[i].y as number) - (v.y as number)) / (chartData[referencePilot].data[i].y as number) * 100;
            v.y = y;

            if (lastValue && (lastValue.y < 0 && y > 0 || lastValue.y > 0 && y < 0)) {
                //add intermediate point        
                const md = new Date(((v.x as Date).valueOf() + lastValue.x.valueOf()) / 2);
                pilotDataPositive.data.push({ x: md, y: 0 });
                pilotDataNegative.data.push({ x: md, y: 0 });
            }

            pilotDataPositive.data.push({ x: v.x as Date, y: y >= 0 ? y : null });
            pilotDataNegative.data.push({ x: v.x as Date, y: y <= 0 ? y : null });

            lastValue = v as { x: Date, y: number };
        }


        t.push(pilotDataPositive);
        t.push(pilotDataNegative);

    }

    chartData = t;

    return (
        <ResponsiveLine
            data={chartData}
            margin={{ top: 50, right: 110, bottom: 50, left: 60 }}
            areaOpacity={0.07}
            colors={[
                'rgb(97, 205, 187)',
                'rgb(244, 117, 96)'
            ]}
            enableArea
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
                legend: 'Difference, %',
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

export default PilotsChartRelative;
