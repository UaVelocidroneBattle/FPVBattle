import { useEffect } from 'react';
import { useGuideLanguage } from '@/hooks/useLanguage';
import { Spinner } from '@/components/ui/spinner';
import { useTeamsStore } from '@/store/teamsStore';
import { translations } from './translations';

/** The limits it mentions are read from the live team settings, so the guide cannot drift from them. */
function TeamsGuidePage() {
    const language = useGuideLanguage();
    const t = translations[language];
    const overview = useTeamsStore((state) => state.overview);
    const overviewState = useTeamsStore((state) => state.overviewState);

    // Visitors have no overview loaded, and signing out drops it
    useEffect(() => {
        if (overviewState === 'Idle') useTeamsStore.getState().fetchOverview();
    }, [overviewState]);

    return (
        <div className="space-y-6 text-slate-200">
            <h1 className="text-2xl font-bold text-slate-200">{t.teamsTitle}</h1>
            <p>{t.teamsIntro}</p>
            {overview ? (
                t.teamsSections(overview).filter((section) => section.items.length > 0).map((section) => (
                    <div key={section.title} className="space-y-3">
                        <p className="font-bold text-slate-100 border-l-2 border-emerald-400 pl-3">{section.title}</p>
                        <ul className="list-disc list-outside pl-6 space-y-2">
                            {section.items.map((item, i) => <li key={i}>{item}</li>)}
                        </ul>
                    </div>
                ))
            ) : overviewState === 'Error' ? (
                <p className="text-red-400">Failed to load the team settings</p>
            ) : (
                <Spinner />
            )}
        </div>
    );
}

export default TeamsGuidePage;
