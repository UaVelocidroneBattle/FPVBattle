import { useEffect } from 'react';
import { Link, useParams } from 'react-router-dom';
import CountryFlag from '@/components/ui/CountryFlag';
import PilotWithAvatar from '@/components/PilotWithAvatar';
import { useCountriesStore } from './countriesStore';

const PANEL_HEIGHT = 'lg:h-[70vh]';

function CountriesPage() {
    const { countryCode } = useParams();

    const countries = useCountriesStore((state) => state.countries);
    const pilots = useCountriesStore((state) => state.pilots);
    const loadingCountries = useCountriesStore((state) => state.loadingCountries);
    const loadingPilots = useCountriesStore((state) => state.loadingPilots);

    useEffect(() => {
        useCountriesStore.getState().fetchCountries();
    }, []);

    useEffect(() => {
        if (countryCode) {
            useCountriesStore.getState().fetchPilots(countryCode);
        }
    }, [countryCode]);

    return (
        <div className="flex flex-col lg:flex-row gap-6">
            <div className={`lg:w-72 shrink-0 flex flex-col ${PANEL_HEIGHT}`}>
                <div className="flex items-center gap-3 px-3 pb-2 border-b border-slate-700 text-xs uppercase tracking-wider text-slate-500">
                    <span className="flex-1">Country</span>
                    <span>Pilots</span>
                </div>

                <div className="scrollbar-slim flex flex-col gap-1 pt-1 overflow-y-auto">
                    {loadingCountries && <p className="px-3 py-2 text-sm text-slate-400">Loading countries…</p>}

                    {countries.map((country) => (
                        <Link
                            key={country.countryCode}
                            to={`/statistics/countries/${country.countryCode}`}
                            className={`flex items-center gap-3 px-3 py-2 rounded transition-colors ${
                                country.countryCode === countryCode
                                    ? 'bg-emerald-500/10 text-emerald-400'
                                    : 'text-slate-200 hover:bg-slate-700/50'
                            }`}
                        >
                            <CountryFlag countryCode={country.countryCode ?? ''} className="text-lg shrink-0" />
                            <span className="flex-1 truncate text-sm">{country.countryName}</span>
                            <span className="text-sm tabular-nums text-slate-400">{country.pilotsCount}</span>
                        </Link>
                    ))}
                </div>
            </div>

            <div className={`flex-1 min-w-0 flex flex-col ${PANEL_HEIGHT}`}>
                <div className="invisible flex items-center gap-3 px-3 pb-2 border-b border-transparent text-xs">
                    Spacer
                </div>

                <div className="scrollbar-slim flex-1 pt-1 overflow-y-auto">
                    {!countryCode && (
                        <p className="text-sm text-slate-400">Select a country to see its pilots.</p>
                    )}

                    {countryCode && (
                        <>
                            {loadingPilots && <p className="text-sm text-slate-400">Loading pilots…</p>}

                            <div className="grid grid-cols-2 sm:grid-cols-3 xl:grid-cols-4 gap-x-3 gap-y-4">
                                {pilots.map((pilot) => (
                                    <PilotWithAvatar
                                        key={pilot.pilotId}
                                        name={pilot.pilotName ?? ''}
                                        countryCode={countryCode}
                                    />
                                ))}
                            </div>
                        </>
                    )}
                </div>
            </div>
        </div>
    );
}

export default CountriesPage;
