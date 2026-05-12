import { useEffect } from 'react'
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

    if (state == 'Loading') {
        return <><Spinner /></>
    }

    if (state == 'Error' || dashboard == null) {
        return <><Error /></>
    }

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
                    <h3 className="text-sm uppercase tracking-wider text-emerald-400 font-medium px-1">
                        TODAY'S LEADERBOARD
                    </h3>
                    <div className="bg-slate-800/50 backdrop-blur-sm border border-slate-700 overflow-hidden">
                        <CurrentLeaderboard trackResults={dashboard.results} />
                    </div>
                </div>

                <div className="flex flex-col gap-3">
                    <h3 className="text-sm uppercase tracking-wider text-emerald-400 font-medium px-1">
                        SEASON LEADERBOARD
                    </h3>
                    <div className="bg-slate-800/50 backdrop-blur-sm border border-slate-700 overflow-hidden">
                        <LeaderBoard leaderBoard={dashboard.leaderboard} />
                    </div>
                </div>
            </div>
        </div>
    );
}

export default PageDashboard;
