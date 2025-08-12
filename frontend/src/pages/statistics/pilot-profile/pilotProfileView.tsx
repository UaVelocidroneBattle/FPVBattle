import { PilotProfileModel, PilotResult } from '@/api/client';
import { Spinner } from '@/components/ui/spinner';
import AchievementsList from './AchievementsList';
import PilotStatsGrid from '@/components/PilotStatsGrid';
import { LoadingStates } from '@/lib/loadingStates';
import { ChartContainer } from '@/components/ChartContainer';
import { lazy } from 'react';

const HeatmapChart = lazy(() => import('./HeatmapChart'));

export interface PilotProfileViewProps {
    profile: PilotProfileModel | null;
    heatmapData: PilotResult[];
    loadingState: LoadingStates;
}

const PilotProfileView = ({ profile, heatmapData, loadingState }: PilotProfileViewProps) => {

    if (loadingState === 'Loading') {
        return <div className="flex justify-center items-center h-64">
            <Spinner />
        </div>;
    }

    if (!profile) return <></>;

    return (
        <>
            {/* Pilot Header */}
            <div className="bg-slate-800 rounded-lg p-6">
                <h1 className="text-3xl font-bold text-white mb-4">{profile.name}</h1>

                {/* Stats Grid */}
                <PilotStatsGrid profile={profile} />
            </div>

            {/* Achievements */}
            <div className="bg-slate-800 rounded-lg p-6">
                <h2 className="text-xl font-semibold text-white mb-4">
                    Achievements ({profile.achievements.length})
                </h2>
                <AchievementsList
                    achievements={profile.achievements}
                />
            </div>

            {/* Heatmap */}
            <div className="bg-slate-800 rounded-lg p-6">
                <h2 className="text-xl font-semibold text-white mb-4">Racing Activity</h2>
                {heatmapData.length > 0 ? (
                    <ChartContainer className="bg-none rounded-lg">
                        <HeatmapChart data={heatmapData} />
                    </ChartContainer>

                ) : (
                    <div className="text-gray-500 text-center py-8">No racing data available</div>
                )}
            </div>
        </>
    );
};

export default PilotProfileView;
