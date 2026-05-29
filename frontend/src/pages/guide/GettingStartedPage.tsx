import { useLanguage } from '@/hooks/useLanguage';
import { translations } from './translations';

function GettingStartedPage() {
    const { language } = useLanguage();
    const t = translations[language];

    return (
        <div className="space-y-6 text-slate-200">
            <h1 className="text-2xl font-bold text-slate-200">{t.gettingStartedTitle}</h1>
            <ol className="list-decimal list-outside pl-6 space-y-4">
                {t.steps.map((step, i) => <li key={i}>{step}</li>)}
            </ol>
        </div>
    );
}

export default GettingStartedPage;
