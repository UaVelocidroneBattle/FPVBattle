import { useLanguage } from '@/hooks/useLanguage';
import { translations } from './translations';

function SupportPage() {
    const { language } = useLanguage();
    const t = translations[language];

    return (
        <div className="space-y-6 text-slate-200">
            <h1 className="text-2xl font-bold text-slate-200">{t.supportTitle}</h1>
            <p>{t.supportText}</p>
        </div>
    );
}

export default SupportPage;
