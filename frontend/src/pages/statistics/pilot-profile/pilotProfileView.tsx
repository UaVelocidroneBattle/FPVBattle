import { PilotProfileModel, PilotResult } from '@/api/client';
import { Spinner } from '@/components/ui/spinner';
import AchievementsList from './AchievementsList';
import PilotStatsGrid from '@/components/PilotStatsGrid';
import { LoadingStates } from '@/lib/loadingStates';
import { ChartContainer } from '@/components/ChartContainer';
import { lazy } from 'react';
import { formatDate } from '@/lib/utils';
import CountryFlag from '@/components/ui/CountryFlag';

const HeatmapChart = lazy(() => import('./HeatmapChart'));

export interface PilotProfileViewProps {
    profile: PilotProfileModel | null;
    heatmapData: PilotResult[];
    loadingState: LoadingStates;
}

function heatmapHeight(data: PilotResult[]): string {
    const firstYear = new Date(data[0].date).getFullYear();
    const lastYear = new Date(data[data.length - 1].date).getFullYear();
    const numYears = lastYear - firstYear + 1;
    const px = Math.max(260, numYears * 170 + (numYears - 1) * 60);
    return `${px}px`;
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
            <div className="bg-slate-800 p-6">
                <h1 className="text-xl sm:text-3xl font-bold text-white mb-3 flex items-center gap-2 sm:gap-3">
                    <CountryFlag countryCode={profile.country} className="text-lg sm:text-2xl" />
                    {profile.name}
                </h1>
                <div className='text-slate-400 mb-4'>
                    <div>First race: {formatDate(profile.firstRaceDate)}</div>
                    <div>Last race: {formatDate(profile.lastRaceDate)}</div>
                </div>
                

                {/* Stats Grid */}
                <PilotStatsGrid profile={profile} />
            </div>

            {/* Achievements */}
            <div className="bg-slate-800 p-6">
                <h2 className="text-xl font-semibold text-white mb-4">
                    Achievements &nbsp;·&nbsp; {profile.achievements.filter(a => a.achievedOn != null).length} of {profile.achievements.length}
                </h2>
                <AchievementsList
                    achievements={profile.achievements}
                />
            </div>

            {/* Heatmap */}
            <div className="bg-slate-800 p-6 hidden sm:block">
                <h2 className="text-xl font-semibold text-white mb-8">Racing Activity</h2>
                {heatmapData.length > 0 ? (
                    <ChartContainer className="bg-none" height={heatmapHeight(heatmapData)}>
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
