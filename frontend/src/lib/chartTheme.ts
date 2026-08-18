import { PartialTheme } from '@nivo/theming';

/**
 * Nivo's default theme is drawn for a light page. This restates it for the dark
 * dashboard so axes, grid lines and legends read as chrome rather than as data —
 * every chart on the site shares it, which is what makes them look like one family.
 */
const AXIS_INK = 'rgba(203, 213, 225, 0.5)';
const GRID_INK = '#94a3b8';

export const chartTheme: PartialTheme = {
    text: { fill: AXIS_INK },
    crosshair: {
        line: {
            stroke: AXIS_INK,
            strokeWidth: 1
        }
    },
    grid: {
        line: {
            stroke: GRID_INK,
            strokeWidth: 1,
            strokeOpacity: 0.2
        }
    },
    axis: {
        domain: { line: { stroke: AXIS_INK } },
        ticks: {
            line: { stroke: AXIS_INK },
            text: { fill: AXIS_INK },
        },
        legend: { text: { fill: AXIS_INK } },
    },
    legends: { text: { fill: AXIS_INK } },
};

/**
 * Series colours, in the order they are handed out. A series keeps its slot as the
 * reader changes range, so the colour never means something different from one
 * moment to the next.
 */
export const seriesColors = [
    'rgb(97, 205, 187)',
    'rgb(244, 117, 96)',
    'rgb(232, 193, 160)',
    'rgb(151, 227, 213)',
];
