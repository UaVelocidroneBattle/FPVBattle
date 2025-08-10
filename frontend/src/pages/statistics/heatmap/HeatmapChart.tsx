import { PilotResult } from '@/api/client';
import { CalendarDatum, ResponsiveCalendarCanvas } from '@nivo/calendar'
import { Theme } from '@nivo/core';

interface HeatmapChartProps {
    data: PilotResult[]
}

const theme: Theme = {
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

    return <>
        <ResponsiveCalendarCanvas
            data={d}
            from={d[0].day}
            to={d[d.length - 1].day}
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