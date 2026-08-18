import CountryFlag from '@/components/ui/CountryFlag';
import { CountryPilotsModel } from '@/api/client';

interface CountryLeaderboardProps {
    countryPilots: CountryPilotsModel[];
    totalCountries: number;
}

function CountryLeaderboard({ countryPilots, totalCountries }: CountryLeaderboardProps) {
    const topCountries = [...countryPilots]
        .sort((a, b) => (b.pilots ?? 0) - (a.pilots ?? 0))
        .slice(0, 10);
    const maxPilots = topCountries[0]?.pilots ?? 1;
    const remaining = totalCountries - topCountries.length;

    return (
        <div className="bg-slate-800/50 backdrop-blur-sm border border-slate-700 p-6 flex flex-col gap-5 w-full lg:w-[28rem] shrink-0">
            <div className="flex items-center gap-4 pb-3 border-b border-slate-700 text-xs uppercase tracking-wider text-slate-500">
                <span className="w-6">#</span>
                <span className="flex-1">Country</span>
                <span>Pilots</span>
            </div>

            {topCountries.map((row, i) => {
                const rank = i + 1;
                return (
                    <div key={row.countryCode ?? row.country ?? rank} className="flex flex-col gap-1.5">
                        <div className="flex items-center gap-4">
                            <span className="w-6 text-sm font-bold tabular-nums text-slate-500">
                                {String(rank).padStart(2, '0')}
                            </span>
                            <CountryFlag countryCode={row.countryCode ?? ''} className="text-base" />
                            <span className="flex-1 text-sm text-slate-200 truncate">{row.country}</span>
                            <span className="text-sm text-white font-medium tabular-nums">{row.pilots ?? 0}</span>
                        </div>
                        <div className="h-1 bg-slate-700 ml-10">
                            <div
                                className="h-full bg-emerald-500"
                                style={{ width: `${((row.pilots ?? 0) / maxPilots) * 100}%` }}
                            />
                        </div>
                    </div>
                );
            })}

            {remaining > 0 && (
                <div className="text-xs text-slate-500 pt-1">… and {remaining} more countries on the board.</div>
            )}
        </div>
    );
}

export default CountryLeaderboard;
