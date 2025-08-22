import { usePilotsStore, usePilotsResults, usePilotResultsLoadingState } from '@/store/pilotsStore';
import { useSelectedPilotsStore, useIsMaxPilotsReached } from '@/store/selectedPilotsStore';
import { useEffect, lazy, useCallback } from 'react';
import { useSearchParams } from 'react-router';
import { useShallow } from 'zustand/shallow';
import { Button } from '@/components/ui/button';
import { Plus } from 'lucide-react';
import PilotSelectors from './PilotSelectors';
import { ChartContainer } from '@/components/ChartContainer';
import { Spinner } from '@/components/ui/spinner';
import { Error } from "@/components/ui/error.tsx";

const PilotsChartAbsolute = lazy(() => import('./PilotsChartAbsolute'))
const PilotsChartRelative = lazy(() => import('./PilotsChartRelative'))


const PagePilots = () => {
    const [searchParams, setSearchParams] = useSearchParams();

    const { state: pilotsState, pilots, fetchPilots, fetchPilotResults } = usePilotsStore(
        useShallow((state) => ({
            state: state.state,
            pilots: state.pilots,
            fetchPilots: state.fetchPilots,
            fetchPilotResults: state.fetchPilotResults
        }))
    );
    const { pilots: selectedPilots, selectPilot, addPilot, clearPilots } = useSelectedPilotsStore(
        useShallow((state) => ({
            pilots: state.pilots,
            selectPilot: state.selectPilot,
            addPilot: state.addPilot,
            clearPilots: state.clearPilots
        }))
    ); //Array of selected pilots. If pilot is not selected for particular combobox, there is a null in array
    const maxPilotsReached = useIsMaxPilotsReached();
    const pilotData = usePilotsResults(selectedPilots);
    const pilotResultsState = usePilotResultsLoadingState(selectedPilots);

    // Update URL when store changes
    const updateUrlFromStore = useCallback(() => {
        const params = new URLSearchParams();
        const validPilots = selectedPilots.filter(p => p !== null);

        if (validPilots.length > 0) {
            params.set('pilot1', validPilots[0]);
        }
        if (validPilots.length > 1) {
            params.set('pilot2', validPilots[1]);
        }

        setSearchParams(params);
    }, [selectedPilots, setSearchParams]);

    // Sync URL parameters with store
    useEffect(() => {
        if (pilotsState === 'Loaded' && pilots.length > 0) {
            const urlPilot1 = searchParams.get('pilot1');
            const urlPilot2 = searchParams.get('pilot2');
            const urlPilots = [urlPilot1, urlPilot2].filter((p): p is string => p !== null && pilots.includes(p));

            const hasUrlPilots = urlPilots.length > 0;
            const hasStorePilots = selectedPilots.some(p => p !== null);

            if (hasUrlPilots) {
                // URL has valid pilots - only sync if they're different from store
                const storePilots = selectedPilots.filter(p => p !== null);
                const urlPilotsMatch = urlPilots.length === storePilots.length &&
                    urlPilots.every((pilot, index) => pilot === storePilots[index]);

                if (!urlPilotsMatch) {
                    // URL pilots are different - sync to store
                    clearPilots();
                    urlPilots.forEach((pilot, index) => {
                        if (index === 0) {
                            selectPilot(pilot, 0);
                            fetchPilotResults(pilot);
                        } else {
                            addPilot();
                            selectPilot(pilot, 1);
                            fetchPilotResults(pilot);
                        }
                    });
                }
            } else if (hasStorePilots && !hasUrlPilots) {
                // Store has pilots but URL doesn't - sync store to URL
                updateUrlFromStore();
            }
        }
    }, [pilotsState, pilots, searchParams]);

    useEffect(() => {
        if (pilotsState == 'Idle' || pilotsState == 'Error') {
            fetchPilots();
        }
    }, [pilotsState, fetchPilots]);

    const pilotChanged = (index: number) => (pilot: string) => {
        selectPilot(pilot, index);
        fetchPilotResults(pilot);
    };

    const handleAddPilot = () => {
        addPilot();
    };

    if (pilotsState == 'Idle') return <></>;

    if (pilotsState == 'Loading') return <Spinner></Spinner>

    if (pilotsState == 'Error') return <Error></Error>

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
                        onClick={handleAddPilot}
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
                <ChartContainer className="bg-none">
                    <PilotsChartRelative pilots={selectedPilots} results={pilotData}></PilotsChartRelative>
                </ChartContainer>
            )}
            <ChartContainer className="bg-none mt-4">
                <PilotsChartAbsolute pilots={selectedPilots} results={pilotData}></PilotsChartAbsolute>
            </ChartContainer>
        </div >
    </>
}

export default PagePilots;
