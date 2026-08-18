import { useEffect, useMemo, useState } from 'react'
import { Navigate, useParams } from 'react-router-dom';
import LeaderBoard from '../../components/LeaderBoard';
import { getCompetitionStore } from '../../store/competitionStore';
import { useShallow } from 'zustand/shallow';
import CurrentCompetition from './CurrentCompetition';
import CurrentLeaderboard, { LeagueLeaderboard } from '@/components/CurrentLeaderBoard';
import { Spinner } from "@/components/ui/spinner.tsx";
import { Error } from "@/components/ui/error.tsx";
import { useUrlDateSync } from './useUrlDateSync';
import { useCups } from '@/hooks/useCups';
import { LeagueLeaderboardModel } from '@/api/client';
import { useHighlightedPilot } from '@/hooks/useHighlightedPilot';
import CountryFilter, { countryOptionsOf } from '@/components/CountryFilter';
import { useUrlCountry } from '@/hooks/useUrlCountry';

const TEN_MINUTES = 10 * 60 * 1000;

/**
 * Today's board for a single country: only that country's pilots, numbered from
 * one again, each keeping in brackets the position it held on the full board —
 * the reading the global rating uses for its country leaderboards. Grouped, the
 * kept position is the one from the pilot's league; flat, everyone is ranked
 * together, so it is the one from the whole day.
 */
function countryLeaderboard(groups: LeagueLeaderboardModel[], countryCode: string, flat: boolean): LeagueLeaderboard[] {
    const positionsOnFullBoard = new Map(
        groups
            .flatMap(group => group.results)
            .sort((a, b) => (a.trackTime ?? 0) - (b.trackTime ?? 0))
            .map((result, index) => [result.playerName, index + 1] as const)
    );

    return groups
        .map(group => ({
            ...group,
            results: group.results
                .filter(result => result.country === countryCode)
                .map((result, index) => flat
                    ? { ...result, originalRank: positionsOnFullBoard.get(result.playerName) }
                    : { ...result, localRank: index + 1, originalRank: result.localRank }),
        }))
        .filter(group => group.results.length > 0);
}

const pilotCount = (groups: { results: unknown[] }[]) =>
    groups.reduce((total, group) => total + group.results.length, 0);

function CupCompetition({ cupId }: { cupId: string }) {
    const useStore = getCompetitionStore(cupId);

    useUrlDateSync(cupId);

    const { state, data: overview, fetch: fetchData, selectedDate, selectDate } = useStore(
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
            getCompetitionStore(cupId).getState().refresh();
        }, TEN_MINUTES);
        return () => clearInterval(interval);
    }, [cupId]);

    const [flat, setFlat] = useState(false);

    const highlightPilotName = useHighlightedPilot();

    // Only today's board is filtered, so only the countries flying today are offered.
    const countryOptions = useMemo(
        () => countryOptionsOf((overview?.leaderboard ?? []).flatMap(group => group.results)),
        [overview]
    );
    const [country, setCountry] = useUrlCountry(countryOptions);

    if (state == 'Loading') {
        return <><Spinner /></>
    }

    if (state == 'Error' || overview == null) {
        return <><Error /></>
    }

    const leagueColors = new Map<string, string>(
        overview.competition?.leagues?.definitions
            ?.filter((d): d is typeof d & { name: string; color: string } => !!d.name && !!d.color)
            .map(d => [d.name, d.color]) ?? []
    );

    const leaguesEnabled = overview.competition?.leagues?.enabled ?? false;

    const leaderboard = country.code
        ? countryLeaderboard(overview.leaderboard, country.code, flat)
        : overview.leaderboard;
    const todayPilots = pilotCount(leaderboard);
    const seasonPilots = pilotCount(overview.seasonLeaderboard);

    return (
        <div className="flex flex-col gap-8">
            <div className="bg-slate-800/50 backdrop-blur-sm border border-slate-700 overflow-hidden">
                <CurrentCompetition
                    cupId={cupId}
                    overview={overview}
                    selectedDate={selectedDate}
                    onDateChange={selectDate}
                />
            </div>

            <div className="grid lg:grid-cols-2 gap-8">
                <div className="flex flex-col gap-3">
                    <div className="flex items-center justify-between">
                        <h3 className="text-sm uppercase tracking-wider text-emerald-400 font-medium pl-1 flex items-baseline gap-2">
                            TODAY'S LEADERBOARD
                            {todayPilots > 0 && (
                                <span className="text-xs text-slate-500 normal-case tracking-normal font-normal">{todayPilots} pilots</span>
                            )}
                        </h3>
                        <div className="flex items-center gap-4">
                            <CountryFilter options={countryOptions} value={country} onChange={setCountry} />
                            {leaguesEnabled && (
                                <>
                                    <div className="h-4 w-px bg-slate-600" />
                                    <button
                                        role="switch"
                                        aria-checked={flat}
                                        onClick={() => setFlat(f => !f)}
                                        className="flex items-center gap-1.5 cursor-pointer"
                                    >
                                        <span className="text-xs text-slate-400">Flat</span>
                                        <span className={`relative inline-flex h-4 w-7 items-center rounded-full transition-colors duration-200 ${flat ? 'bg-emerald-500' : 'bg-slate-600'}`}>
                                            <span className={`inline-block h-3 w-3 rounded-full bg-white shadow transition-transform duration-200 ${flat ? 'translate-x-3.5' : 'translate-x-0.5'}`} />
                                        </span>
                                    </button>
                                </>
                            )}
                        </div>
                    </div>
                    <CurrentLeaderboard leaderboard={leaderboard} leagueColors={leagueColors} flat={flat} isEnded={overview.competition?.state === 1} highlightPilotName={highlightPilotName} />
                </div>

                <div className="flex flex-col gap-3">
                    <h3 className="text-sm uppercase tracking-wider text-emerald-400 font-medium px-1 flex items-baseline gap-2">
                        SEASON LEADERBOARD
                        {seasonPilots > 0 && (
                            <span className="text-xs text-slate-500 normal-case tracking-normal font-normal">{seasonPilots} pilots</span>
                        )}
                    </h3>
                    <LeaderBoard leaderboard={overview.seasonLeaderboard} leagueColors={leagueColors} highlightPilotName={highlightPilotName} />
                </div>
            </div>
        </div>
    );
}

function CompetitionPage() {
    const { cupId } = useParams();
    const { loadingState, findCup } = useCups();

    if (loadingState === "Error") {
        return <Error />;
    }

    if (loadingState !== "Loaded") {
        return <Spinner />;
    }

    const cup = findCup(cupId);

    // This route matches any unclaimed path, so anything that is not a cup
    // belongs on the landing page.
    if (!cup) {
        return <Navigate to="/" replace />;
    }

    return <CupCompetition key={cup.id} cupId={cup.id} />;
}

export default CompetitionPage;
