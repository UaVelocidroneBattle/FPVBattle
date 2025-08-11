import { Spinner } from '@/components/ui/spinner';
import { usePilotsStore, usePilotResults, usePilotResultLoadingState } from '@/store/pilotsStore';
import { useHeatmapStore } from '@/store/heatmapStore';
import { useEffect, Suspense, lazy } from 'react';
import ComboBox from '@/components/ComboBox';

const HeatmapChart = lazy(() => import('./HeatmapChart'))

const pilotKey = (pilot: string) => pilot;
const pilotLabel = (pilot: string) => pilot;

const PageHeatmap = () => {

    const pilotsState = usePilotsStore(state => state.state);
    const pilots = usePilotsStore(state => state.pilots);
    const fetchPilots = usePilotsStore(state => state.fetchPilots);
    const fetchPilotResults = usePilotsStore(state => state.fetchPilotResults);
    const { currentPilot, choosePilot } = useHeatmapStore();
    const heatMap = usePilotResults(currentPilot);
    const pilotResultsState = usePilotResultLoadingState(currentPilot);

    useEffect(() => {
        if (pilotsState == 'Idle' || pilotsState == 'Error') {
            fetchPilots();
        }
    }, [pilotsState]);

    const selectPilot = (pilot: string) => {
        choosePilot(pilot);
        fetchPilotResults(pilot);
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
