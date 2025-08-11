import { Spinner } from '@/components/ui/spinner';
import { usePilotsStore, usePilotsResults, usePilotResultsLoadingState } from '@/store/pilotsStore';
import { useSelectedPilotsStore, useIsMaxPilotsReached } from '@/store/selectedPilotsStore';
import { useEffect, Suspense, lazy } from 'react';
import { Button } from '@/components/ui/button';
import { Plus } from 'lucide-react';
import PilotSelectors from './PilotSelectors';

const PilotsChartAbsolute = lazy(() => import('./PilotsChartAbsolute'))
const PilotsChartRelative = lazy(() => import('./PilotsChartRelative'))


const PagePilots = () => {
    const pilotsState = usePilotsStore(state => state.state);
    const pilots = usePilotsStore(state => state.pilots);
    const fetchPilots = usePilotsStore(state => state.fetchPilots);
    const fetchPilotResults = usePilotsStore(state => state.fetchPilotResults);
    const { pilots: selectedPilots, selectPilot, addPilot } = useSelectedPilotsStore(); //Array of selected pilots. If pilot is not selected for particular combobox, there is a null in array
    const maxPilotsReached = useIsMaxPilotsReached();
    const pilotData = usePilotsResults(selectedPilots);
    const pilotResultsState = usePilotResultsLoadingState(selectedPilots);

    useEffect(() => {
        if (pilotsState == 'Idle' || pilotsState == 'Error') {
            fetchPilots();
        }
    }, [pilotsState]);

    const pilotChanged = (index: number) => (pilot: string) => {
        selectPilot(pilot, index);
        fetchPilotResults(pilot);
    }

    if (pilotsState == 'Idle') return <></>;

    if (pilotsState == 'Loading') return <h2 className='text-center text-2xl text-green-500'>🚁 Loading</h2>

    if (pilotsState == 'Error') return <h2>Error</h2>

    return <>
        <div className="mb-4 text-sm text-gray-200">
            Compare lap times between two pilots on a track. Use the controls to choose pilots.
        </div>
        <div className='flex'>
            <PilotSelectors
                selectedPilots={selectedPilots}
                pilots={pilots}
                onPilotChanged={pilotChanged}
            />
            {!maxPilotsReached &&
                <div className='flex-row'>
                    <Button
                        variant="outline"
                        size="icon"
                        onClick={() => {
                            addPilot()
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
                    {selectedPilots.filter(p => p !== null).length > 1 && <>
                        <div className='bg-slate-200 rounded-lg w-full overflow-hidden min-w-0' style={{ height: '600px' }}>
                            <PilotsChartRelative pilots={selectedPilots} results={pilotData}></PilotsChartRelative>
                        </div>
                    </>}
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
