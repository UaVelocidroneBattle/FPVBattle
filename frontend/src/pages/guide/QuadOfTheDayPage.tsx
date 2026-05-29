import { useLanguage } from '@/hooks/useLanguage';
import { translations } from './translations';

function QuadOfTheDayPage() {
    const { language } = useLanguage();
    const t = translations[language];

    return (
        <div className="space-y-6 text-slate-200">
            <h1 className="text-2xl font-bold text-slate-200">{t.quadOfTheDayTitle}</h1>
            {t.quadOfTheDayParagraphs.map((p, i) => <p key={i}>{p}</p>)}
        </div>
    );
}

export default QuadOfTheDayPage;
