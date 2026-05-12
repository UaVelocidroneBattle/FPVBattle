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

    useEffect(() => {
        if (pilotsState === 'Idle' || pilotsState === 'Error') {
            usePilotsStore.getState().fetchPilots();
        }
    }, [pilotsState]);

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
        navigate(`/statistics/profile/${encodeURIComponent(selected)}`, { replace: !pilot });
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
