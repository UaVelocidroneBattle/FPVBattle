import { useAppDispatch, useAppSelector } from '@/lib/hooks';
import { Spinner } from '@/components/ui/spinner';
import { fetchPilots, selectPilots, selectPilotsState, selectPilotResultLoadingState, fetchPilotResults, selectPilotResults } from '@/lib/features/pilots/pilotsSlice';
import { useEffect, Suspense, lazy } from 'react';
import ComboBox from '@/components/ComboBox';
import { choosePilot, selectCurrentPilot } from '@/lib/features/heatmap/heatmapSlice';

const HeatmapChart = lazy(() => import('./HeatmapChart'))

const pilotKey = (pilot: string) => pilot;
const pilotLabel = (pilot: string) => pilot;

const PageHeatmap = () => {

    const dispatch = useAppDispatch();
    const pilotsState = useAppSelector(selectPilotsState);
    const pilots = useAppSelector(selectPilots);
    const currentPilot = useAppSelector(selectCurrentPilot);
    const heatMap = useAppSelector(state => selectPilotResults(state, currentPilot));
    const pilotResultsState = useAppSelector(state => selectPilotResultLoadingState(state, currentPilot));


    useEffect(() => {
        if (pilotsState == 'Idle' || pilotsState == 'Error') {
            dispatch(fetchPilots());
        }
    }, [pilotsState, dispatch]);

    const selectPilot = (pilot: string) => {
        dispatch(choosePilot(pilot));
        dispatch(fetchPilotResults(pilot));
    }

    if (pilotsState == 'Idle') return <></>;

    if (pilotsState == 'Loading') return <h2 className='text-center text-2xl text-green-500'>🚁 Loading</h2>

    if (pilotsState == 'Error') return <h2>Error</h2>

    return <>
        <ComboBox defaultCaption='Select a pilot'
            items={pilots}
            getKey={pilotKey}
            getLabel={pilotLabel}
            onSelect={selectPilot}
            value={currentPilot!}></ComboBox>

        <div className='py-6'>

            {pilotResultsState == 'Loading' && <>
                <Spinner></Spinner>
            </>
            }

            {pilotResultsState == 'Loaded' && <>
                <div className='bg-none rounded-lg' style={{ height: '600px' }}>
                    <Suspense fallback={<div>Loading...</div>}>
                        <HeatmapChart data={heatMap} />
                    </Suspense>
                </div>
            </>
            }
        </div>

    </>
}

export default PageHeatmap;
