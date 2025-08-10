import { useAppDispatch, useAppSelector } from '@/lib/hooks';
import { Spinner } from '@/components/ui/spinner';
import { fetchPilots, selectPilots, selectPilotsState, fetchPilotResults, selectPilotsResults, selectPilotResultsLoadingState } from '@/lib/features/pilots/pilotsSlice';
import { useEffect, Suspense, lazy } from 'react';
import ComboBox from '@/components/ComboBox';
import { addPilot, selectSelectedPilots, selectIsMaxPilotsReached, selectPilot } from '@/lib/features/selectedPilots/selectedPilotsSlice';
import { Button } from '@/components/ui/button';
import { Plus } from 'lucide-react';

const PilotsChartAbsolute = lazy(() => import('@/pages/Pilots/PilotsChartAbsolute'))
const PilotsChartRelative = lazy(() => import('@/pages/Pilots/PilotsChartRelative'))

const pilotKey = (pilot: string) => pilot;
const pilotLabel = (pilot: string) => pilot;

const PagePilots = () => {
    const dispatch = useAppDispatch();
    const pilotsState = useAppSelector(selectPilotsState);
    const pilots = useAppSelector(selectPilots);
    const selectedPilots = useAppSelector(selectSelectedPilots);
    const maxPilotsReached = useAppSelector(selectIsMaxPilotsReached);
    const pilotData = useAppSelector(state => selectPilotsResults(state, selectedPilots));
    const pilotResultsState = useAppSelector(state => selectPilotResultsLoadingState(state, selectedPilots));


    useEffect(() => {
        if (pilotsState == 'Idle' || pilotsState == 'Error') {
            dispatch(fetchPilots());
        }
    }, [pilotsState, dispatch]);

    const pilotChanged = (index: number) => (pilot: string) => {
        dispatch(selectPilot({ index: index, pilotName: pilot }));
        dispatch(fetchPilotResults(pilot));
    }

    if (pilotsState == 'Idle') return <></>;

    if (pilotsState == 'Loading') return <h2 className='text-center text-2xl text-green-500'>🚁 Loading</h2>

    if (pilotsState == 'Error') return <h2>Error</h2>

    return <>
        <div className='flex'>
            {selectedPilots.map((sp, index) => <div key={index} className='flex-row'>
                <ComboBox defaultCaption='Select a pilot'
                    items={pilots}
                    getKey={pilotKey}
                    getLabel={pilotLabel}
                    onSelect={pilotChanged(index)}
                    value={sp}></ComboBox>
            </div>)}
            {!maxPilotsReached &&
                <div className='flex-row'>
                    <Button
                        variant="outline"
                        size="icon"
                        onClick={() => {
                            dispatch(addPilot())
                        }}
                        className="h-10 w-10"
                    >
                        <Plus className="h-4 w-4" />
                    </Button>
                </div>}
        </div>



        <div className='py-6'>

            {pilotResultsState == 'Loading' && <>
                <Spinner></Spinner>
            </>
            }

            {pilotResultsState == 'Loaded' && <>
                <Suspense fallback={<div>Loading...</div>}>
                    <div className='bg-slate-200 rounded-lg w-full overflow-hidden min-w-0' style={{ height: '600px' }}>
                        <PilotsChartRelative pilots={selectedPilots} results={pilotData}></PilotsChartRelative>
                    </div>
                    <div className='bg-slate-200 rounded-lg mt-4 w-full overflow-hidden min-w-0' style={{ height: '600px' }}>
                        <PilotsChartAbsolute pilots={selectedPilots} results={pilotData}></PilotsChartAbsolute>
                    </div>
                </Suspense>
            </>
            }
        </div >
    </>
}

export default PagePilots;
