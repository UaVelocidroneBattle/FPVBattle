import { type ReactNode } from 'react';
import { useLanguage, type Language } from '@/hooks/useLanguage';

const link = (href: string, label: string) => (
    <a href={href} className="text-emerald-400 hover:text-emerald-300 transition-colors" target="_blank" rel="noopener noreferrer">
        {label}
    </a>
);

interface RulesContent {
    title: string;
    steps: ReactNode[];
    howItWorksTitle: string;
    howItWorksItems: ReactNode[];
    dayStreakTitle: string;
    dayStreakIntro: string;
    dayStreakItems: string[];
    freezeTitle: string;
    freezeIntro: string;
    freezeLogic: string;
    freezeItems: ReactNode[];
    quadOfTheDayTitle: string;
    quadOfTheDayParagraphs: string[];
    achievementsTitle: string;
    achievementsText: string;
    supportTitle: string;
    supportText: ReactNode;
    flyAway: string;
}

const translations: Record<Language, RulesContent> = {
    ua: {
        title: 'Інструкція',
        steps: [
            <>Завантажуємо Velocidrone з {link('https://www.velocidrone.com/shop', 'офіційного сайту')}</>,
            <>Перевіряємо, щоб у вашому акаунті Velocidrone був встановлений український прапор. Це можна переглянути й змінити (за потреби) у вашому {link('https://www.velocidrone.com/profile', 'профілі')}</>,
            <>Вмикаємо автоматичну публікацію результатів у налаштуваннях симулятора:<br /><span className="text-slate-400">{'Options → Main Settings → Auto Leaderbord Time Update: Yes'}</span></>,
            'Обираємо один з режимів: Nemesis або Single Player',
            'Обираємо Game type: Single Class – Three Laps Race',
        ],
        howItWorksTitle: 'Як це працює',
        howItWorksItems: [
            'Сервіс автоматично оновлює результати кожні 10 хвилин і підтягне ваш результат, якщо він з\'явиться.',
            'Щодня о 00:00 (UTC) сервіс випадковим чином обирає трек із доступного набору локацій і треків. У деяких випадках трек може бути змінений, якщо він не підходить для змагань.',
            'Через 24 години, о 00:00 (UTC), сервіс підбиває підсумки дня та нараховує бали відповідно до зайнятого місця.',
            'Сезон починається 1-го числа кожного місяця і завершується 1-го числа наступного місяця.',
            'Дуже важливо оцінювати кожен трек. Для цього сервіс публікує голосування (наразі тільки в Telegram-каналі). Результати голосування впливають на те, чи буде цей трек обраний знову.',
        ],
        dayStreakTitle: 'Day streak',
        dayStreakIntro: 'У системі працює механіка day streak — лічильник днів, протягом яких ви літаєте без перерв.',
        dayStreakItems: [
            'Якщо ви пропускаєте день — streak скидається.',
            'Якщо у вас є freeze, streak не скидається, але витрачається freeze.',
        ],
        freezeTitle: 'Day streak freeze',
        freezeIntro: 'Freeze (заморозки) потрібні, щоб зберегти ваш day streak у разі пропуску дня.',
        freezeLogic: 'Логіка нарахування:',
        freezeItems: [
            '1 freeze за кожні 30 днів streak',
            <>або ви можете стати нашим підписником на {link('https://patreon.com/FPVBattle', 'Patreon')} — патронам нараховуються додаткові фрізи.</>,
        ],
        quadOfTheDayTitle: 'Квад дня',
        quadOfTheDayParagraphs: [
            'Іноді разом із треком сервіс також оголошує Квад дня. Якщо ви летите на обраному дроні в цей день — отримаєте бонусні бали наприкінці.',
            'Ви можете летіти на будь-якому дроні, але бонусні бали нараховуються лише за виконання умови.',
        ],
        achievementsTitle: 'Achievements',
        achievementsText: 'У системі є набір achievements, який постійно розширюється. Вони видаються за досягнення певної кількості day streak, першого місця в гонці, тощо.',
        supportTitle: 'Підтримати нас',
        supportText: <>Ви можете підтримати проєкт на нашому {link('https://patreon.com/FPVBattle', 'Patreon')} для покриття різних витрат (хостинг, домен, кава, тощо).</>,
        flyAway: 'Полетіли! 🚀',
    },
    en: {
        title: 'Instructions',
        steps: [
            <>Download Velocidrone from the {link('https://www.velocidrone.com/shop', 'official website')}</>,
            <>
                <p>For Ukrainians: make sure the Ukrainian flag is selected in your Velocidrone account. You can check and change it (if needed) in your {link('https://www.velocidrone.com/profile', 'profile')}.</p>
                <p className="mt-2">For others: you need to join our {link('https://discord.gg/FrpC2WV8Cw', 'Discord')} and provide your Velocidrone pilot name so we can add you.</p>
            </>,
            <>Enable automatic result publishing in the simulator settings:<br /><span className="text-slate-400">{'Options → Main Settings → Auto Leaderbord Time Update: Yes'}</span></>,
            'Select one of the modes: Nemesis or Single Player',
            'Select Game type: Single Class – Three Laps Race',
        ],
        howItWorksTitle: 'How it works',
        howItWorksItems: [
            'The service automatically updates results every 10 minutes and will pick up your result once it appears.',
            'Every day at 00:00 (UTC) the service randomly selects a track from the available set of locations and tracks. In some cases the track may be changed if it is not suitable for competition.',
            'After 24 hours, at 00:00 (UTC), the service calculates the day\'s results and awards points based on final placement.',
            'The season starts on the 1st of each month and ends on the 1st of the following month.',
            'It is very important to rate each track. The service publishes a poll (currently only in the Telegram channel). Poll results affect whether the track will be selected again.',
        ],
        dayStreakTitle: 'Day streak',
        dayStreakIntro: 'The system features a day streak mechanic — a counter of consecutive days you have flown without missing one.',
        dayStreakItems: [
            'If you miss a day, the streak resets.',
            'If you have a freeze, the streak is not reset but a freeze is consumed.',
        ],
        freezeTitle: 'Day streak freeze',
        freezeIntro: 'Freezes are used to preserve your day streak when you miss a day.',
        freezeLogic: 'How freezes are earned:',
        freezeItems: [
            '1 freeze for every 30-day streak',
            <>or you can become our supporter on {link('https://patreon.com/FPVBattle', 'Patreon')} — Patreon supporters receive additional freezes.</>,
        ],
        quadOfTheDayTitle: 'Quad of the Day',
        quadOfTheDayParagraphs: [
            'The system may occasionally announce a Quad of the Day alongside the Track of the Day. If you fly using the selected quad on that day, you\'ll receive bonus points at the end.',
            'You can still fly any quad you want, but you won\'t earn bonus points unless you use the selected one.',
        ],
        achievementsTitle: 'Achievements',
        achievementsText: 'The system has a growing set of achievements, awarded for reaching certain day streak milestones, finishing in first place, and more.',
        supportTitle: 'Support us',
        supportText: <>You can support the project on our {link('https://patreon.com/FPVBattle', 'Patreon')} to help cover various costs (hosting, domain, coffee, etc.).</>,
        flyAway: "Let's fly! 🚀",
    },
};

function SectionTitle({ children }: { children: ReactNode }) {
    return <h2 className="text-xl font-bold text-emerald-400">{children}</h2>;
}

function LanguageToggle({ language, setLanguage }: { language: Language; setLanguage: (lang: Language) => void }) {
    return (
        <div className="flex gap-1 border border-slate-600 p-0.5 text-sm font-medium">
            {(['ua', 'en'] as Language[]).map((lang) => (
                <button
                    key={lang}
                    onClick={() => setLanguage(lang)}
                    className={`px-3 py-1 transition-colors uppercase ${
                        language === lang ? 'bg-slate-600 text-slate-200' : 'text-slate-500 hover:text-slate-300'
                    }`}
                >
                    {lang}
                </button>
            ))}
        </div>
    );
}

function PageRules() {
    const { language, setLanguage } = useLanguage();
    const t = translations[language];

    return (
        <div className="bg-slate-800/50 backdrop-blur-sm border border-slate-700 overflow-hidden p-6 max-w-4xl mx-auto">
            <div className="flex items-center justify-between mb-6">
                <SectionTitle>{t.title}</SectionTitle>
                <LanguageToggle language={language} setLanguage={setLanguage} />
            </div>

            <div className="space-y-8 text-slate-200">
                <ol className="list-decimal list-outside pl-6 space-y-6">
                    {t.steps.map((step, i) => <li key={i}>{step}</li>)}
                </ol>

                <SectionTitle>{t.howItWorksTitle}</SectionTitle>
                <ul className="list-disc list-outside pl-6 space-y-6">
                    {t.howItWorksItems.map((item, i) => <li key={i}>{item}</li>)}
                </ul>

                <SectionTitle>{t.dayStreakTitle}</SectionTitle>
                <p>{t.dayStreakIntro}</p>
                <ul className="list-disc list-outside pl-6 space-y-2">
                    {t.dayStreakItems.map((item, i) => <li key={i}>{item}</li>)}
                </ul>

                <SectionTitle>{t.freezeTitle}</SectionTitle>
                <div className="space-y-2">
                    <p>{t.freezeIntro}</p>
                    <p>{t.freezeLogic}</p>
                    <ul className="list-disc list-outside pl-6 space-y-2">
                        {t.freezeItems.map((item, i) => <li key={i}>{item}</li>)}
                    </ul>
                </div>

                <SectionTitle>{t.quadOfTheDayTitle}</SectionTitle>
                <div className="space-y-2">
                    {t.quadOfTheDayParagraphs.map((p, i) => <p key={i}>{p}</p>)}
                </div>

                <SectionTitle>{t.achievementsTitle}</SectionTitle>
                <p>{t.achievementsText}</p>

                <SectionTitle>{t.supportTitle}</SectionTitle>
                <p>{t.supportText}</p>

                <p className="text-xl font-bold text-emerald-400 text-center mt-8">{t.flyAway}</p>
            </div>
        </div>
    );
}

export default PageRules;
