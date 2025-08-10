import { useEffect } from 'react'
import LeaderBoard from '../../components/LeaderBoard';
import { fetch, selectState, selectData } from '../../lib/features/dashboard/dashboardSlice';
import { useAppDispatch, useAppSelector } from '../../lib/hooks';
import CurrentCompetition from './CurrentCompetition';

const PageDashboard: React.FC = () => {

    const dispatch = useAppDispatch();
    const state = useAppSelector(selectState);
    const dashboard = useAppSelector(selectData);

    useEffect(() => {
        dispatch(fetch());
    }, [dispatch]);

    if (state == 'Loading') {
        return <>
            <h2 className='text-2xl text-center text-gray-400'>Loading... 🚁</h2>
        </>
    }

    if (state == 'Error' || dashboard == null) {
        return <>
            <h2 className='text-2xl text-center text-red-400'>🤦 something happened</h2>
        </>
    }

    return <>
        <div className="grid lg:grid-cols-2 gap-8">
            {/* Current Competition */}
            <div className="bg-slate-800/50 backdrop-blur-sm rounded-2xl border border-slate-700 overflow-hidden">
                <CurrentCompetition dashboard={dashboard}></CurrentCompetition>
            </div>

            {/* Tournament Leaderboard */}
            <div className="bg-slate-800/50 backdrop-blur-sm rounded-2xl border border-slate-700 overflow-hidden">
                <div className="px-6 py-8 border-b border-slate-700/50">
                    <h3 className="text-sm uppercase tracking-wider text-emerald-400 font-medium">
                        LEADERBOARD
                    </h3>
                </div>
                <LeaderBoard leaderBoard={dashboard.leaderboard}></LeaderBoard>
            </div>
        </div>
    </>

}

export default PageDashboard;