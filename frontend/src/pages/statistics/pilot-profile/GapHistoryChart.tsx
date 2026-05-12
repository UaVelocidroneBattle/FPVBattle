import { PilotRatingHistoryPoint } from '@/api/client';
import { ResponsiveLine } from '@nivo/line';
import { PartialTheme } from '@nivo/theming';
import React from 'react';

interface GapHistoryChartProps {
    history: PilotRatingHistoryPoint[];
}

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
    tooltip: {
        container: {
            background: '#1e293b',
            color: '#cbd5e1',
            borderRadius: '6px',
            padding: '8px 12px',
            fontSize: '13px',
        }
    }
};

const GapHistoryChart = ({ history }: GapHistoryChartProps) => {
    const points = history
        .filter(p => p.gapPercent != null)
        .map(p => ({ x: new Date(p.date), y: p.gapPercent as number }));

    const data = points.slice(0, -1).map((point, i) => {
        const next = points[i + 1];
        const improving = next.y <= point.y;
        return {
            id: `segment-${i}`,
            color: improving ? 'rgb(52, 211, 153)' : 'rgb(248, 113, 113)',
            data: [point, next]
        };
    });

    return (
        <ResponsiveLine
            data={data}
            theme={theme}
            margin={{ top: 10, right: 20, bottom: 60, left: 55 }}
            xScale={{
                type: 'time',
                format: '%Y-%m-%d',
                precision: 'day',
                useUTC: false
            }}
            yScale={{
                type: 'linear',
                min: 'auto',
                max: 'auto',
            }}
            xFormat="time:%b %d, %Y"
            yFormat=" >-.2f"
            curve="monotoneX"
            colors={d => d.color}
            markers={[{
                axis: 'y',
                value: 0,
                lineStyle: {
                    stroke: 'rgba(203, 213, 225, 0.4)',
                    strokeWidth: 1,
                    strokeDasharray: '6 4'
                },
                legend: 'Leader',
                legendPosition: 'top-right',
                textStyle: { fill: 'rgba(203, 213, 225, 0.4)', fontSize: 11 }
            }]}
            axisBottom={{
                tickValues: [],
                legend: 'Time',
                legendOffset: 36,
                legendPosition: 'middle',
            }}
            axisLeft={{
                legend: 'Gap to leader, %',
                legendOffset: -48,
                legendPosition: 'middle',
                format: v => `${Math.round(v as number)}`,
                tickValues: 5
            }}
            enablePoints
            pointSize={10}
            pointColor={{ from: 'seriesColor' }}
            pointBorderWidth={2}
            pointBorderColor={{ from: 'seriesColor' }}
            tooltip={({ point }) => {
                const date = new Date(point.data.x);
                const formatted = `${String(date.getDate()).padStart(2, '0')}.${String(date.getMonth() + 1).padStart(2, '0')}.${String(date.getFullYear()).slice(2)}`;
                return (
                    <div style={theme.tooltip?.container as React.CSSProperties} className="text-center">
                        <div className="font-bold text-sm">{(point.data.y as number).toFixed(2)}%</div>
                        <div className="text-slate-400 text-xs">{formatted}</div>
                    </div>
                );
            }}
            useMesh
            enableCrosshair
        />
    );
};

export default GapHistoryChart;
