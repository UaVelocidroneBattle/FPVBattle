import { useLanguage } from '@/hooks/useLanguage';
import { translations } from './translations';

function HowItWorksPage() {
    const { language } = useLanguage();
    const t = translations[language];

    return (
        <div className="space-y-6 text-slate-200">
            <h1 className="text-2xl font-bold text-slate-200">{t.howItWorksTitle}</h1>
            <ul className="list-disc list-outside pl-6 space-y-4">
                {t.howItWorksItems.map((item, i) => <li key={i}>{item}</li>)}
            </ul>
        </div>
    );
}

export default HowItWorksPage;
