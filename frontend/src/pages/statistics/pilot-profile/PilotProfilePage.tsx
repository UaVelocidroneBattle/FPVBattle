import { useEffect } from 'react';
import { useSearchParams } from 'react-router';
import {
    usePilotProfileStore,
    usePilotProfile,
    usePilotHeatmapData,
    usePilotProfilePageLoadingState,
} from '@/store/pilotProfileStore';
import { usePilotsStore } from '@/store/pilotsStore';
import { useShallow } from 'zustand/shallow';
import ComboBox from '@/components/ComboBox';
import PilotProfileView from './pilotProfileView';
import { Spinner } from '@/components/ui/spinner';
import { Error } from '@/components/ui/error';



const PilotProfilePage = () => {
    const [searchParams, setSearchParams] = useSearchParams();

    //heatmap store should be combinded with profile store
    const { currentPilot, choosePilot } = usePilotProfileStore(
        useShallow((state) => ({
            currentPilot: state.currentPilot,
            choosePilot: state.choosePilot
        }))
    );

    const profile = usePilotProfile(currentPilot || null);
    const heatmapData = usePilotHeatmapData(currentPilot || null);
    const loadingState = usePilotProfilePageLoadingState(currentPilot || null);

    const pilotKey = (pilot: string) => pilot;
    const pilotLabel = (pilot: string) => pilot;

    const { state: pilotsState, pilots } = usePilotsStore(
        useShallow((state) => ({
            state: state.state,
            pilots: state.pilots
        }))
    );

    // Sync URL parameter with store on mount and when pilots list changes
    useEffect(() => {
        if (pilotsState === 'Loaded' && pilots.length > 0) {
            const urlPilot = searchParams.get('pilot');
            
            if (urlPilot && pilots.includes(urlPilot)) {
                // URL has valid pilot - sync to store if different
                if (currentPilot !== urlPilot) {
                    choosePilot(urlPilot);
                }
            } else if (currentPilot && !urlPilot) {
                // Store has pilot but URL doesn't - sync to URL
                setSearchParams({ pilot: currentPilot });
            }
        }
    }, [pilotsState, pilots, searchParams, currentPilot, choosePilot, setSearchParams]);

    useEffect(() => {
        if (pilotsState == 'Idle' || pilotsState == 'Error') {
            const { fetchPilots } = usePilotsStore.getState();
            fetchPilots();
        }
    }, [pilotsState]);

    // Handle pilot selection - update both store and URL
    const handlePilotSelect = (pilot: string) => {
        choosePilot(pilot);
        setSearchParams({ pilot });
    };


    if (pilotsState == 'Idle') return <></>;

    if (pilotsState == 'Loading') return <Spinner/>;

    if (pilotsState == 'Error') return <Error/>;

    return (
        <>
            <div className="flex items-center gap-4 mb-4">
                <ComboBox defaultCaption='Select a pilot'
                    items={pilots}
                    getKey={pilotKey}
                    getLabel={pilotLabel}
                    onSelect={handlePilotSelect}
                    value={currentPilot}></ComboBox>
            </div>

            <div className="space-y-6">
                <PilotProfileView profile={profile} heatmapData={heatmapData} loadingState={loadingState} />
            </div>
        </>
    );
};

export default PilotProfilePage;