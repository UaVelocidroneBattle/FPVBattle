import { RelativeChartDataPoint } from './PilotsChartRelative';

interface RelativeChartTooltipProps {
    point: {
        data: RelativeChartDataPoint;
    };
    pilots: (string | null)[];
}

const RelativeChartTooltip = ({ point, pilots }: RelativeChartTooltipProps) => {
    const percentage = point.data.y as number;
    const isFaster = percentage > 0;
    const absoluteDifference = (point.data as RelativeChartDataPoint).absoluteDifference as number;

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
            <div style={{ whiteSpace: 'nowrap' }}>
                <strong>{pilots[0] || 'Unknown'}</strong> is {Math.abs(percentage).toFixed(2)}% {isFaster ? 'faster' : 'slower'} than <strong>{pilots[1] || 'Unknown'}</strong> ({absoluteDifference.toFixed(2)}s)
            </div>
        </div>
    );
};

export default RelativeChartTooltip;
