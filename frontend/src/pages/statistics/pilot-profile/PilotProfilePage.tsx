import { useEffect } from 'react';
import { useParams, useNavigate } from 'react-router';
import { useShallow } from 'zustand/shallow';
import { usePilotProfileStore, usePilotProfile, usePilotHeatmapData, usePilotProfilePageLoadingState } from '@/store/pilotProfileStore';
import { usePilotsStore } from '@/store/pilotsStore';
import ComboBox from '@/components/ComboBox';
import PilotProfileView from './pilotProfileView';
import { Spinner } from '@/components/ui/spinner';
import { Error } from '@/components/ui/error';


function PilotProfilePage() {
    const { pilot } = useParams<{ pilot: string }>();
    const navigate = useNavigate();

    const { state: pilotsState, pilots } = usePilotsStore(
        useShallow((state) => ({
            state: state.state,
            pilots: state.pilots,
        }))
    );

    // Loads on every visit until it succeeds, but never retries in a loop while the server is unreachable
    useEffect(() => {
        const { state, fetchPilots } = usePilotsStore.getState();
        if (state === 'Idle' || state === 'Error') {
            fetchPilots();
        }
    }, []);

    useEffect(() => {
        if (!pilot) return;
        const { fetchPilotProfile, fetchPilotHeatmapData } = usePilotProfileStore.getState();
        fetchPilotProfile(pilot);
        fetchPilotHeatmapData(pilot);
    }, [pilot]);

    const profile = usePilotProfile(pilot ?? null);
    const heatmapData = usePilotHeatmapData(pilot ?? null);
    const loadingState = usePilotProfilePageLoadingState(pilot ?? null);

    const handlePilotSelect = (selected: string) => {
        navigate(`/pilot/${encodeURIComponent(selected)}`, { replace: !pilot });
    };

    if (pilotsState === 'Idle') return <></>;
    if (pilotsState === 'Loading') return <Spinner />;
    if (pilotsState === 'Error') return <Error />;

    return (
        <>
            <div className="flex items-center gap-4 mb-4">
                <ComboBox
                    defaultCaption='Select a pilot'
                    items={pilots}
                    getKey={(p) => p}
                    getLabel={(p) => p}
                    onSelect={handlePilotSelect}
                    value={pilot ?? null}
                />
            </div>

            <div className="space-y-6">
                <PilotProfileView profile={profile} heatmapData={heatmapData} loadingState={loadingState} />
            </div>
        </>
    );
}

export default PilotProfilePage;
