import { useEffect } from 'react';
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
import { useUrlPilotProfileSync } from './useUrlPilotProfileSync';



const PilotProfilePage = () => {
    useUrlPilotProfileSync();

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


    useEffect(() => {
        if (pilotsState == 'Idle' || pilotsState == 'Error') {
            const { fetchPilots } = usePilotsStore.getState();
            fetchPilots();
        }
    }, [pilotsState]);

    const handlePilotSelect = (pilot: string) => {
        choosePilot(pilot);
    };


    if (pilotsState == 'Idle') return <></>;

    if (pilotsState == 'Loading') return <Spinner />;

    if (pilotsState == 'Error') return <Error />;

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