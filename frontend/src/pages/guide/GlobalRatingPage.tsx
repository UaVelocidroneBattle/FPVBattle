import { useLanguage } from '@/hooks/useLanguage';
import { translations } from './translations';

function GlobalRatingPage() {
    const { language } = useLanguage();
    const t = translations[language];

    return (
        <div className="space-y-6 text-slate-200">
            <h1 className="text-2xl font-bold text-slate-200">{t.globalRatingTitle}</h1>
            <div className="space-y-4">
                {t.globalRatingParagraphs.map((block, i) => <div key={i}>{block}</div>)}
            </div>
        </div>
    );
}

export default GlobalRatingPage;
