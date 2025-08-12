import { PilotResult } from '@/api/client';
import { CalendarDatum, ResponsiveCalendarCanvas } from '@nivo/calendar'

interface HeatmapChartProps {
    data: PilotResult[]
}

const theme = {
    text: {
        fill: "#94a3b8"
    }
};

const HeatmapChart = ({ data }: HeatmapChartProps) => {
    const d = data.map(i => ({
        day: new Date(i.date!).toISOString().split('T')[0],
        value: i.points
    } as CalendarDatum));

    if (data.length == 0) return <></>;

    const from = new Date(data[0].date);
    const to = new Date(data[data.length - 1].date);

    return <>
        <ResponsiveCalendarCanvas
            data={d}
            from={from}
            to={to}
            emptyColor="#0000"
            colors={['#125348', '#156154', '#1d8775', '#22a18b', '#2ed3b8']}
            margin={{ top: 0, right: 20, bottom: 40, left: 20 }}
            yearSpacing={70}
            dayBorderWidth={1}
            dayBorderColor="#516585"

            theme={
                theme
            }
            legends={[
                {
                    anchor: 'bottom-right',
                    direction: 'row',
                    translateY: 0,
                    itemCount: 4,
                    itemWidth: 42,
                    itemHeight: 36,
                    itemsSpacing: 14,
                    itemDirection: 'right-to-left'
                }
            ]}
        />

    </>
}

export default HeatmapChart;