import { DashboardModel } from "@/api/client";
import ClickableTrackName from "@/components/ClickableTrackName";
import CurrentLeaderboard from "@/components/CurrentLeaderBoard";
import VelocdroneResultLink from "@/components/VelocidroneResultsLink";

interface ICurrentCompetitionProps {
    dashboard: DashboardModel
}

const CurrentCompetition = ({ dashboard }: ICurrentCompetitionProps) => {

    if (dashboard.competition == null) {
        return <>
            <div className="px-6 py-8 border-b border-slate-700/50">
                <div className="flex flex-col space-y-1">
                    <h3 className="text-sm uppercase tracking-wider text-emerald-400 font-medium mb-2">
                        Nothing is happening right now
                    </h3>
                </div>
            </div>
        </>
    }

    return <>
        <div className="px-6 py-8 border-b border-slate-700/50">
            <div className="flex flex-col space-y-2">
                <h3 className="text-sm uppercase tracking-wider text-emerald-400 font-medium mb-2">
                    Today's track:
                </h3>
                <ClickableTrackName mapName={dashboard.competition.mapName} trackName={dashboard.competition.trackName} />
                <VelocdroneResultLink trackInfo={{ MapId: dashboard.competition.mapId, TrackId: dashboard.competition.trackId }}></VelocdroneResultLink>
            </div>
        </div>
        <CurrentLeaderboard trackResults={dashboard.results} />
    </>
}

export default CurrentCompetition;