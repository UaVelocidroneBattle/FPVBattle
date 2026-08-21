import { PilotProfileModel, PilotResult } from '@/api/client';
import { Spinner } from '@/components/ui/spinner';
import AchievementsList from './AchievementsList';
import RatingSection from './RatingSection';
import { LoadingStates } from '@/lib/loadingStates';
import { ChartContainer } from '@/components/ChartContainer';
import { lazy, useMemo, useState } from 'react';
import { formatDate } from '@/lib/utils';
import CountryFlag from '@/components/ui/CountryFlag';
import { countryName } from '@/lib/countries';
import GapHistorySection from './GapHistorySection';

const HeatmapChart = lazy(() => import('./HeatmapChart'));

export interface PilotProfileViewProps {
    profile: PilotProfileModel | null;
    heatmapData: PilotResult[];
    loadingState: LoadingStates;
}

function extractYears(data: PilotResult[]): number[] {
    const years = new Set(data.map(r => new Date(r.date).getFullYear()));
    return Array.from(years).sort((a, b) => b - a);
}

const PilotProfileView = ({ profile, heatmapData, loadingState }: PilotProfileViewProps) => {
    const years = useMemo(() => extractYears(heatmapData), [heatmapData]);
    const [selectedYear, setSelectedYear] = useState<number | null>(null);
    const activeYear = selectedYear ?? years[0] ?? null;
    const yearData = useMemo(
        () => heatmapData.filter(r => new Date(r.date).getFullYear() === activeYear),
        [heatmapData, activeYear]
    );

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
                <h1 className="text-xl sm:text-3xl font-bold text-white flex items-center flex-wrap gap-2 sm:gap-3">
                    {profile.name}
                    {profile.country && (
                        <span className="flex items-center gap-1.5 text-xs sm:text-sm font-medium text-slate-400 bg-slate-700/60 border border-slate-600 rounded-full pl-3 pr-4 py-1">
                            <CountryFlag countryCode={profile.country} className="text-xs sm:text-sm" />
                            {countryName(profile.country)}
                        </span>
                    )}
                </h1>
                <div className="flex items-center flex-wrap gap-x-2 gap-y-1 text-xs sm:text-sm text-slate-400 mt-3 mb-4">
                    <span>First race: {formatDate(profile.firstRaceDate)}</span>
                    <span aria-hidden="true" className="text-slate-600">·</span>
                    <span>Last race: {formatDate(profile.lastRaceDate)}</span>
                </div>
            </div>

            {/* Rating */}
            <RatingSection profile={profile} />

            {/* Achievements */}
            <div className="bg-slate-800 p-6">
                <h2 className="text-xl font-semibold text-white mb-4">
                    Achievements &nbsp;·&nbsp; {profile.achievements.filter(a => a.achievedOn != null).length} of {profile.achievements.length}
                </h2>
                <AchievementsList
                    achievements={profile.achievements}
                />
            </div>

            {/* Rating Gap History */}
            {profile.classRatings.some(c => c.ratingHistory.length > 1) && (
                <GapHistorySection classRatings={profile.classRatings} />
            )}

            {/* Heatmap */}
            <div className="bg-slate-800 p-6 hidden sm:block">
                <div className="flex items-center justify-between mb-8">
                    <h2 className="text-xl font-semibold text-white">Racing Activity</h2>
                    {activeYear !== null && (
                        <div className="flex items-center gap-1">
                            <button
                                onClick={() => setSelectedYear(years[years.indexOf(activeYear) + 1])}
                                disabled={years.indexOf(activeYear) === years.length - 1}
                                className="px-3 py-1 text-sm rounded transition-colors duration-150 bg-slate-700 text-slate-400 hover:bg-slate-600 hover:text-white disabled:opacity-30 disabled:cursor-not-allowed disabled:hover:bg-slate-700 disabled:hover:text-slate-400"
                            >
                                &#8592;
                            </button>
                            <span className="px-3 py-1 text-sm rounded bg-emerald-500 text-white">{activeYear}</span>
                            <button
                                onClick={() => setSelectedYear(years[years.indexOf(activeYear) - 1])}
                                disabled={years.indexOf(activeYear) === 0}
                                className="px-3 py-1 text-sm rounded transition-colors duration-150 bg-slate-700 text-slate-400 hover:bg-slate-600 hover:text-white disabled:opacity-30 disabled:cursor-not-allowed disabled:hover:bg-slate-700 disabled:hover:text-slate-400"
                            >
                                &#8594;
                            </button>
                        </div>
                    )}
                </div>
                {activeYear !== null ? (
                    <ChartContainer className="bg-none" height="250px">
                        <HeatmapChart data={yearData} year={activeYear} />
                    </ChartContainer>
                ) : (
                    <div className="text-gray-500 text-center py-8">No racing data available</div>
                )}
            </div>
        </>
    );
};

export default PilotProfileView;
