import {
    getApiPilotStatisticsCount,
    getApiPilotStatisticsNewPilots,
    getApiPilotStatisticsParticipation,
} from '../api/client';
import { createRangedChartStore } from './rangedChartStore';

/**
 * How many pilots there are, day by day. Half a year is enough to show the shape of
 * the growth without flattening it against years of earlier history.
 */
export const usePilotsCountStore = createRangedChartStore(
    [
        { key: '6months', label: '6 months', size: 182 },
        { key: 'year', label: '1 year', size: 365 },
        { key: 'all', label: 'All time', size: null },
    ],
    async (days) => (await getApiPilotStatisticsCount({ query: { days: days ?? undefined } })).data
);

/**
 * How many pilots arrived each month. Twelve bars fit a year of seasons — the summer
 * and winter swings only mean something once a whole one is on screen.
 */
export const useNewPilotsStore = createRangedChartStore(
    [
        { key: 'year', label: '1 year', size: 12 },
        { key: 'all', label: 'All time', size: null },
    ],
    async (months) => (await getApiPilotStatisticsNewPilots({ query: { months: months ?? undefined } })).data
);

/**
 * How many pilots flew each day, per cup. The same figures the admin dashboard shows,
 * on the same scales.
 */
export const useParticipationStore = createRangedChartStore(
    [
        { key: 'month', label: 'Month', size: 30 },
        { key: '6months', label: '6 months', size: 182 },
        { key: 'year', label: '1 year', size: 365 },
        { key: 'all', label: 'All time', size: null },
    ],
    async (days) => (await getApiPilotStatisticsParticipation({ query: { days: days ?? undefined } })).data
);
