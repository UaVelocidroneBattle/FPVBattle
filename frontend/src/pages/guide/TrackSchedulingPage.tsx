import { useGuideLanguage } from '@/hooks/useLanguage';
import { translations } from './translations';

function TrackSchedulingPage() {
    const language = useGuideLanguage();
    const t = translations[language];

    return (
        <div className="space-y-6 text-slate-200">
            <h1 className="text-2xl font-bold text-slate-200">{t.trackSchedulingTitle}</h1>
            <p>{t.trackSchedulingIntro}</p>
            <ul className="list-disc list-outside pl-6 space-y-2">
                {t.trackSchedulingItems.map((item, i) => <li key={i}>{item}</li>)}
            </ul>
        </div>
    );
}

export default TrackSchedulingPage;
