import { lazy } from 'react';
import {
    useNewPilotsStore,
    useParticipationStore,
    usePilotsCountStore,
} from '@/store/pilotsStatisticsStore';
import { ChartSection } from './ChartSection';

// Nivo is a heavy dependency and none of it is needed until this page is opened.
const PilotsCountChart = lazy(() => import('./PilotsCountChart'));
const NewPilotsChart = lazy(() => import('./NewPilotsChart'));
const ParticipationChart = lazy(() => import('./ParticipationChart'));

/**
 * The community by the numbers: how many pilots there are, how many keep arriving,
 * and how many actually fly.
 */
const PilotsPage = () => {
    return (
        <div className="flex flex-col gap-10">
            <ChartSection
                title="Pilots count"
                description="The total number of pilots in FPV Battle, day by day."
                store={usePilotsCountStore}
                isEmpty={(data) => data.days.length === 0}
                height="400px"
            >
                {(data) => <PilotsCountChart data={data} />}
            </ChartSection>

            <ChartSection
                title="New pilots"
                description="Pilots flying their first competition each month."
                store={useNewPilotsStore}
                isEmpty={(data) => data.months.length === 0}
                height="400px"
            >
                {(data) => <NewPilotsChart data={data} />}
            </ChartSection>

            <ChartSection
                title="Pilot participation"
                description="How many pilots flew each day, in each class."
                store={useParticipationStore}
                isEmpty={(data) => data.days.length === 0}
                height="420px"
            >
                {(data) => <ParticipationChart data={data} />}
            </ChartSection>
        </div>
    );
};

export default PilotsPage;
