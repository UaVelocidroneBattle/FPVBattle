import { useEffect } from 'react'
import LeaderBoard from '../../components/LeaderBoard';
import { useDashboardStore } from '../../store/dashboardStore';
import { useShallow } from 'zustand/shallow';
import CurrentCompetition from './CurrentCompetition';
import { Spinner } from "@/components/ui/spinner.tsx";
import { Error } from "@/components/ui/error.tsx";

const PageDashboard: React.FC = () => {

    const { state, data: dashboard, fetch: fetchData } = useDashboardStore(
        useShallow((state) => ({
            state: state.state,
            data: state.data,
            fetch: state.fetch
        }))
    );

    useEffect(() => {
        if (state === 'Idle' || state === 'Error') {
            fetchData();
        }
    }, [state, fetchData]);



    if (state == 'Loading') {
        return <>
            <Spinner></Spinner>
        </>
    }

    if (state == 'Error' || dashboard == null) {
        return <>
            <Error></Error>
        </>
    }
    return <>
        <div className="grid lg:grid-cols-2 gap-8">
            <div className="bg-slate-800/50 backdrop-blur-sm rounded-2xl border border-slate-700 overflow-hidden pb-4">
                <CurrentCompetition dashboard={dashboard}></CurrentCompetition>
            </div>

            <div className="bg-slate-800/50 backdrop-blur-sm rounded-2xl border border-slate-700 overflow-hidden pb-4">
                <div className="px-6 py-8 border-b border-slate-700/50">
                    <h3 className="text-sm uppercase tracking-wider text-emerald-400 font-medium mb-2">
                        LEADERBOARD
                    </h3>
                </div>
                <LeaderBoard leaderBoard={dashboard.leaderboard}></LeaderBoard>
            </div>
        </div>
    </>

}

export default PageDashboard;