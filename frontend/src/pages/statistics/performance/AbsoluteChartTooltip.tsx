import { Point, LineSeries } from '@nivo/line';

interface AbsoluteChartTooltipProps {
    point: Point<LineSeries>;
    filteredData: LineSeries[];
    pilots: (string | null)[];
}

const AbsoluteChartTooltip = ({ point, filteredData, pilots }: AbsoluteChartTooltipProps) => {
    const currentDate = point.data.x as Date;
    const pilot1Time = filteredData[0]?.data.find(d => (d.x as Date).valueOf() === currentDate.valueOf())?.y as number;
    const pilot2Time = filteredData[1]?.data.find(d => (d.x as Date).valueOf() === currentDate.valueOf())?.y as number;

    // Check if second pilot exists and has data
    const hasSecondPilot = pilots[1] && filteredData[1] && pilot2Time !== undefined;

    return (
        <div style={{
            background: '#ffffff',
            padding: '9px 12px',
            border: '1px solid #ccc',
            borderRadius: '4px',
            boxShadow: '0 2px 4px rgba(0,0,0,0.1)',
            color: '#333333',
            fontSize: '12px'
        }}>
            <div style={{ whiteSpace: 'nowrap', marginBottom: hasSecondPilot ? '2px' : '0' }}>
                <strong>{pilots[0] || 'Unknown'} </strong>{pilot1Time?.toFixed(2)}s
            </div>
            {hasSecondPilot && (
                <div style={{ whiteSpace: 'nowrap' }}>
                    <strong>{pilots[1]} </strong>{pilot2Time.toFixed(2)}s
                </div>
            )}
        </div>
    );
};

export default AbsoluteChartTooltip;
