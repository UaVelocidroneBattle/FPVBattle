import { useLanguage } from '@/hooks/useLanguage';
import { translations } from './translations';

function FreezePage() {
    const { language } = useLanguage();
    const t = translations[language];

    return (
        <div className="space-y-6 text-slate-200">
            <h1 className="text-2xl font-bold text-slate-200">{t.freezeTitle}</h1>
            <p>{t.freezeIntro}</p>
            <div className="space-y-3">
                <p className="font-bold text-slate-100 border-l-2 border-emerald-400 pl-3">{t.freezeLogic}</p>
                <ul className="list-disc list-outside pl-6 space-y-2">
                    {t.freezeItems.map((item, i) => <li key={i}>{item}</li>)}
                </ul>
            </div>
        </div>
    );
}

export default FreezePage;
