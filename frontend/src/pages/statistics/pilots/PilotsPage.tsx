import { usePilotsStore, usePilotsResults, usePilotResultsLoadingState } from '@/store/pilotsStore';
import { useSelectedPilotsStore, useIsMaxPilotsReached } from '@/store/selectedPilotsStore';
import { useEffect, lazy } from 'react';
import { useShallow } from 'zustand/shallow';
import { Button } from '@/components/ui/button';
import { Plus } from 'lucide-react';
import PilotSelectors from './PilotSelectors';
import { ChartContainer } from '@/components/ChartContainer';
import { Spinner } from '@/components/ui/spinner';

const PilotsChartAbsolute = lazy(() => import('./PilotsChartAbsolute'))
const PilotsChartRelative = lazy(() => import('./PilotsChartRelative'))


const PagePilots = () => {
    const { state: pilotsState, pilots, fetchPilots, fetchPilotResults } = usePilotsStore(
        useShallow((state) => ({
            state: state.state,
            pilots: state.pilots,
            fetchPilots: state.fetchPilots,
            fetchPilotResults: state.fetchPilotResults
        }))
    );
    const { pilots: selectedPilots, selectPilot, addPilot } = useSelectedPilotsStore(
        useShallow((state) => ({
            pilots: state.pilots,
            selectPilot: state.selectPilot,
            addPilot: state.addPilot
        }))
    ); //Array of selected pilots. If pilot is not selected for particular combobox, there is a null in array
    const maxPilotsReached = useIsMaxPilotsReached();
    const pilotData = usePilotsResults(selectedPilots);
    const pilotResultsState = usePilotResultsLoadingState(selectedPilots);

    useEffect(() => {
        if (pilotsState == 'Idle' || pilotsState == 'Error') {
            fetchPilots();
        }
    }, [pilotsState, fetchPilots]);

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
                        className="w-10 bg-slate-800/50 hover:bg-slate-700/50 text-slate-200 hover:text-slate-200 border-slate-700 "
                    >
                        <Plus className="h-4 w-4" />
                    </Button>
                </div>}
        </div>



        <div className='py-6'>
            {pilotResultsState == 'Loading' && (
                <Spinner></Spinner>
            )}
            {selectedPilots.filter(p => p !== null).length > 1 && (
                <ChartContainer>
                    <PilotsChartRelative pilots={selectedPilots} results={pilotData}></PilotsChartRelative>
                </ChartContainer>
            )}
            <ChartContainer className="bg-slate-200 rounded-lg mt-4">
                <PilotsChartAbsolute pilots={selectedPilots} results={pilotData}></PilotsChartAbsolute>
            </ChartContainer>
        </div >
    </>
}

export default PagePilots;
