import { lazy, Suspense } from 'react';
import CountryLeaderboard from './CountryLeaderboard';
import { Spinner } from '@/components/ui/spinner';
import { useIsDesktop } from './useIsDesktop';
import { LandingDataModel } from '@/api/client';

const WorldMap = lazy(() => import('./WorldMap'));

interface PilotMapSectionProps {
    data: LandingDataModel;
}

function PilotMapSection({ data }: PilotMapSectionProps) {
    const isDesktop = useIsDesktop();
    const countryPilots = data.countryPilots ?? [];
    const totalCountries = data.totalCountries ?? countryPilots.length;

    return (
        <div className="flex flex-col gap-4 mt-8">
            {isDesktop && (
                <h3 className="text-sm uppercase tracking-wider text-emerald-400 font-medium pl-1">Pilot map</h3>
            )}
            <div className="flex flex-col lg:flex-row gap-6">
                {isDesktop && (
                    <div className="relative flex-1 min-w-0 bg-slate-800/50 backdrop-blur-sm border border-slate-700 p-6 overflow-hidden">
                        <div
                            className="absolute inset-0 opacity-30 pointer-events-none"
                            style={{
                                backgroundImage:
                                    'linear-gradient(to right, #334155 1px, transparent 1px), linear-gradient(to bottom, #334155 1px, transparent 1px)',
                                backgroundSize: '24px 24px',
                            }}
                        />
                        <Suspense fallback={<Spinner />}>
                            <WorldMap countryPilots={countryPilots} />
                        </Suspense>
                    </div>
                )}
                <CountryLeaderboard countryPilots={countryPilots} totalCountries={totalCountries} />
            </div>
        </div>
    );
}

export default PilotMapSection;
