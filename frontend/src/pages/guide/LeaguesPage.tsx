import { useEffect } from 'react';
import { useLanguage } from '@/hooks/useLanguage';
import { translations } from './translations';
import { useCups } from '@/hooks/useCups';
import { useGlobalRatingStore, LeagueDescriptorModel } from '@/store/globalRatingStore';
import { Spinner } from '@/components/ui/spinner';

/**
 * The seat ladder, ordered top league first, exactly as LeagueService fills it — read live from
 * the cup's configuration so the guide can never drift from what the leagues actually pay out.
 */
function LeagueLadder({ descriptors, spotsLabel, allRemaining }: { descriptors: LeagueDescriptorModel[]; spotsLabel: string; allRemaining: string }) {
    const ordered = [...descriptors].sort((a, b) => (a.order ?? 0) - (b.order ?? 0));

    return (
        <ul className="list-disc list-outside pl-6 space-y-2 mt-3">
            {ordered.map((league) => (
                <li key={league.name}>
                    <span className="font-bold" style={{ color: league.color ?? undefined }}>
                        {league.name.toUpperCase()}
                    </span>
                    {' — '}
                    {league.size ? `${league.size} ${spotsLabel}` : allRemaining}
                </li>
            ))}
        </ul>
    );
}

function LeaguesPage() {
    const { language } = useLanguage();
    const t = translations[language];
    const { defaultCup, loadingState: cupsLoadingState } = useCups();
    const data = useGlobalRatingStore((state) => state.data);
    const ratingLoadingState = useGlobalRatingStore((state) => state.loadingState);

    useEffect(() => {
        if (!defaultCup) return;
        useGlobalRatingStore.getState().fetchRatings(defaultCup.id);
    }, [defaultCup]);

    const loading = cupsLoadingState !== 'Loaded' || ratingLoadingState === 'Idle' || ratingLoadingState === 'Loading';
    const leagueSettings = data?.leagueSettings;

    return (
        <div className="space-y-6 text-slate-200">
            <h1 className="text-2xl font-bold text-slate-200">{t.leaguesTitle}</h1>
            <div className="space-y-4">
                <div>
                    <p>{t.leaguesIntro}</p>
                    {loading ? (
                        <Spinner />
                    ) : ratingLoadingState === 'Error' ? (
                        <p className="text-red-400">Failed to load league configuration</p>
                    ) : leagueSettings?.enabled && leagueSettings.descriptors?.length ? (
                        <>
                            <LeagueLadder descriptors={leagueSettings.descriptors} spotsLabel={t.leaguesSpotsLabel} allRemaining={t.leaguesAllRemaining} />
                            <p className="mt-3">{t.leaguesFooter(leagueSettings.othersName ?? 'Unranked')}</p>
                        </>
                    ) : null}
                </div>
                {t.leaguesParagraphs.map((block, i) => <div key={i}>{block}</div>)}
            </div>
        </div>
    );
}

export default LeaguesPage;
