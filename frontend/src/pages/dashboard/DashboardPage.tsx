import { useEffect, useState } from 'react'
import LeaderBoard from '../../components/LeaderBoard';
import { getDashboardStore } from '../../store/dashboardStore';
import { useShallow } from 'zustand/shallow';
import CurrentCompetition from './CurrentCompetition';
import CurrentLeaderboard from '@/components/CurrentLeaderBoard';
import { Spinner } from "@/components/ui/spinner.tsx";
import { Error } from "@/components/ui/error.tsx";
import { useUrlDateSync } from './useUrlDateSync';

const TEN_MINUTES = 10 * 60 * 1000;

interface DashboardPageProps {
    cupId: string;
}

function PageDashboard({ cupId }: DashboardPageProps) {
    const useStore = getDashboardStore(cupId);

    useUrlDateSync(cupId);

    const { state, data: dashboard, fetch: fetchData, selectedDate, selectDate } = useStore(
        useShallow((state) => ({
            state: state.state,
            data: state.data,
            fetch: state.fetch,
            selectedDate: state.selectedDate,
            selectDate: state.selectDate,
        }))
    );

    useEffect(() => {
        if (state === 'Idle' || state === 'Error') {
            fetchData();
        }
    }, [state, fetchData]);

    useEffect(() => {
        const interval = setInterval(() => {
            getDashboardStore(cupId).getState().refresh();
        }, TEN_MINUTES);
        return () => clearInterval(interval);
    }, [cupId]);

    const [flat, setFlat] = useState(false);

    if (state == 'Loading') {
        return <><Spinner /></>
    }

    if (state == 'Error' || dashboard == null) {
        return <><Error /></>
    }

    const leagueColors = new Map(
        dashboard.competition?.leagues.definitions
            .filter(d => d.color)
            .map(d => [d.name, d.color!]) ?? []
    );

    const leaguesEnabled = dashboard.competition?.leagues.enabled ?? false;

    return (
        <div className="flex flex-col gap-8">
            <div className="bg-slate-800/50 backdrop-blur-sm border border-slate-700 overflow-hidden">
                <CurrentCompetition
                    cupId={cupId}
                    dashboard={dashboard}
                    selectedDate={selectedDate}
                    onDateChange={selectDate}
                />
            </div>

            <div className="grid lg:grid-cols-2 gap-8">
                <div className="flex flex-col gap-3">
                    <div className="flex items-center justify-between">
                        <h3 className="text-sm uppercase tracking-wider text-emerald-400 font-medium pl-1 flex items-baseline gap-2">
                            TODAY'S LEADERBOARD
                            {(() => {
                                const count = dashboard.leaderboard?.reduce((sum, g) => sum + (g.results?.length ?? 0), 0) ?? 0;
                                return count > 0 && <span className="text-xs text-slate-500 normal-case tracking-normal font-normal">{count} pilots</span>;
                            })()}
                        </h3>
                        {leaguesEnabled && (
                            <button
                                onClick={() => setFlat(f => !f)}
                                className={`px-3 py-1 text-sm rounded transition-colors duration-150 ${flat ? 'bg-emerald-500 text-white' : 'bg-slate-700 text-slate-400 hover:bg-slate-600 hover:text-white'}`}
                            >
                                Flat
                            </button>
                        )}
                    </div>
                    <CurrentLeaderboard leaderboard={dashboard.leaderboard} leagueColors={leagueColors} flat={flat} />
                </div>

                <div className="flex flex-col gap-3">
                    <h3 className="text-sm uppercase tracking-wider text-emerald-400 font-medium px-1 flex items-baseline gap-2">
                        SEASON LEADERBOARD
                        {(() => {
                            const count = dashboard.seasonLeaderboard?.reduce((sum, g) => sum + (g.results?.length ?? 0), 0) ?? 0;
                            return count > 0 && <span className="text-xs text-slate-500 normal-case tracking-normal font-normal">{count} pilots</span>;
                        })()}
                    </h3>
                    <LeaderBoard leaderboard={dashboard.seasonLeaderboard} leagueColors={leagueColors} />
                </div>
            </div>
        </div>
    );
}

export default PageDashboard;
