import { type ReactNode } from 'react';
import { type Language } from '@/hooks/useLanguage';

export function link(href: string, label: string): ReactNode {
    return (
        <a href={href} className="text-emerald-400 hover:text-emerald-300 transition-colors" target="_blank" rel="noopener noreferrer">
            {label}
        </a>
    );
}

export const navItems: { path: string; label: Record<Language, string> }[] = [
    { path: 'getting-started', label: { ua: 'Як почати',         en: 'Getting Started'   } },
    { path: 'how-it-works',    label: { ua: 'Як це працює',      en: 'How It Works'      } },
    { path: 'global-rating',   label: { ua: 'Global Rating',     en: 'Global Rating'     } },
    { path: 'leagues',         label: { ua: 'Leagues',           en: 'Leagues'           } },
    { path: 'day-streak',      label: { ua: 'Day streak',        en: 'Day Streak'        } },
    { path: 'freeze',          label: { ua: 'Day streak freeze', en: 'Day Streak Freeze' } },
    { path: 'quad-of-the-day', label: { ua: 'Квад дня',         en: 'Quad of the Day'   } },
    { path: 'achievements',    label: { ua: 'Achievements',      en: 'Achievements'      } },
    { path: 'support',         label: { ua: 'Підтримка',       en: 'Support'         } },
];

export interface RulesContent {
    gettingStartedTitle: string;
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
    leaguesTitle: string;
    leaguesParagraphs: ReactNode[];
    globalRatingTitle: string;
    globalRatingParagraphs: ReactNode[];
    achievementsTitle: string;
    achievementsText: string;
    supportTitle: string;
    supportText: ReactNode;
    flyAway: string;
}

export const translations: Record<Language, RulesContent> = {
    ua: {
        gettingStartedTitle: 'Як почати',
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
            'Іноді разом із треком сервіс також оголошує Квад дня.',
            'Якщо ви летите не на обраному дроні — незалежно від зайнятого місця, отримаєте лише 1 бал наприкінці дня. Крім того, ваш результат буде виключено з розрахунку глобального рейтингу, щоб не спотворювати показники інших пілотів.',
        ],
        leaguesTitle: 'Leagues',
        leaguesParagraphs: [
            <>
                <p>Існує три ліги: <span className="font-bold text-amber-600">BRONZE</span>, <span className="font-bold text-slate-300">SILVER</span> і <span className="font-bold text-yellow-400">GOLD</span>. У кожної ліги обмежена кількість місць:</p>
                <ul className="list-disc list-outside pl-6 space-y-2 mt-3">
                    <li><span className="font-bold text-yellow-400">GOLD</span> — 10 місць</li>
                    <li><span className="font-bold text-slate-300">SILVER</span> — 15 місць</li>
                    <li><span className="font-bold text-amber-600">BRONZE</span> — всі інші пілоти</li>
                </ul>
                <p className="mt-3">У кожної ліги свій окремий лідерборд, медалі за призові місця і нарахування очок. Для пілотів, які не потрапили до Global Rating і не були розподілені в жодну лігу, — окремий лідерборд <span className="font-bold text-slate-400">UNRANKED</span>.</p>
            </>,
            'Після завершення сезону в кінці кожного місяця відбуватиметься перерозподіл пілотів по лігах знову-таки на основі Global Rating.',
            ' Органічний спосіб перейти у вищу лігу — ставати швидшим і підніматися вище в Global Rating. Але бувають ситуації, коли пілот тримає свій темп упродовж усього сезону, проте в його лігу потрапляють кілька більш швидких пілотів — через ліміт місць такий пілот може «зіскочити» в нижчу лігу. І навпаки: якщо більш швидкі пілоти «вилетять» з Global Rating, такий пілот може зайняти їхнє місце у вищій лізі.',
            'Таким чином, винагороджуються пілоти саме за швидкість, а не за фарм очок за місяць.',
        ],
        globalRatingTitle: 'Global Rating',
        globalRatingParagraphs: [
            'Global Rating розраховується раз на тиждень і показує, наскільки в середньому пілот відстає від найкращих у відсотках. Але рахується це не за одним результатом, а за всіма результатами за останні 30 днів. Для кожного треку система дивиться топ-3 результати і рахує їхній середній час.',
            <div>
                <div className="bg-slate-900/60 border border-slate-700 p-4 space-y-1 text-sm font-mono">
                    <p className="text-slate-400 mb-2">Наприклад:</p>
                    <p>Pilot 1 — 50s</p>
                    <p>Pilot 2 — 51s</p>
                    <p>Pilot 3 — 52s</p>
                </div>
                <p className="mt-3">Середній час = <span className="text-emerald-400">51s</span>. Саме від нього рахується відставання. Якщо ваш час 53s, ви приблизно на <span className="text-emerald-400">3.9%</span> повільніші.</p>
            </div>,
            'Система бере всі ваші результати за останні 30 днів і рахує середній відсоток відставання. Саме це число і є вашим Global Rating GAP. Чим менший відсоток, тим вище місце в рейтингу.',
            <div><p className="font-bold text-slate-100 border-l-2 border-emerald-400 pl-3">Чому у першого місця може бути від'ємний GAP?</p><p className="mt-2 pl-3">Бо еталон — це середній час топ-3. Якщо пілот літає швидше за цей середній час, його GAP стає від'ємним. Тобто він швидший за середній рівень топ-3.</p></div>,
            <div><p className="font-bold text-slate-100 border-l-2 border-emerald-400 pl-3">Хто потрапляє в рейтинг?</p><p className="mt-2 pl-3">До таблиці потрапляють тільки пілоти, які літали мінімум 15 днів за останні 30 днів. Це потрібно, щоб рейтинг відображав стабільні результати.</p></div>,
        ],
        achievementsTitle: 'Achievements',
        achievementsText: 'У системі є набір achievements, який постійно розширюється. Вони видаються за досягнення певної кількості day streak, першого місця в гонці, тощо.',
        supportTitle: 'Підтримати нас',
        supportText: <>Ви можете підтримати проєкт на нашому {link('https://patreon.com/FPVBattle', 'Patreon')} для покриття різних витрат (хостинг, домен, кава, тощо).</>,
        flyAway: 'Полетіли! 🚀',
    },
    en: {
        gettingStartedTitle: 'Getting Started',
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
        howItWorksTitle: 'How It Works',
        howItWorksItems: [
            'The service automatically updates results every 10 minutes and will pick up your result once it appears.',
            'Every day at 00:00 (UTC) the service randomly selects a track from the available set of locations and tracks. In some cases the track may be changed if it is not suitable for competition.',
            'After 24 hours, at 00:00 (UTC), the service calculates the day\'s results and awards points based on final placement.',
            'The season starts on the 1st of each month and ends on the 1st of the following month.',
            'It is very important to rate each track. The service publishes a poll (currently only in the Telegram channel). Poll results affect whether the track will be selected again.',
        ],
        dayStreakTitle: 'Day Streak',
        dayStreakIntro: 'The system features a day streak mechanic — a counter of consecutive days you have flown without missing one.',
        dayStreakItems: [
            'If you miss a day, the streak resets.',
            'If you have a freeze, the streak is not reset but a freeze is consumed.',
        ],
        freezeTitle: 'Day Streak Freeze',
        freezeIntro: 'Freezes are used to preserve your day streak when you miss a day.',
        freezeLogic: 'How freezes are earned:',
        freezeItems: [
            '1 freeze for every 30-day streak',
            <>or you can become our supporter on {link('https://patreon.com/FPVBattle', 'Patreon')} — Patreon supporters receive additional freezes.</>,
        ],
        quadOfTheDayTitle: 'Quad of the Day',
        quadOfTheDayParagraphs: [
            'The system may occasionally announce a Quad of the Day alongside the Track of the Day.',
            'If you fly on a different quad — regardless of your finishing position — you will only receive 1 point at the end of the day. Your result will also be excluded from global rating calculation to avoid distorting other pilots\' statistics.',
        ],
        leaguesTitle: 'Leagues',
        leaguesParagraphs: [
            <>
                <p>There are three leagues: <span className="font-bold text-amber-600">BRONZE</span>, <span className="font-bold text-slate-300">SILVER</span>, and <span className="font-bold text-yellow-400">GOLD</span>. Each league has a limited number of spots:</p>
                <ul className="list-disc list-outside pl-6 space-y-2 mt-3">
                    <li><span className="font-bold text-yellow-400">GOLD</span> — 10 spots</li>
                    <li><span className="font-bold text-slate-300">SILVER</span> — 15 spots</li>
                    <li><span className="font-bold text-amber-600">BRONZE</span> — all remaining pilots</li>
                </ul>
                <p className="mt-3">Each league has its own separate leaderboard, medals for top placements, and point scoring. Pilots who are not in the Global Rating and were not assigned to any league have a separate <span className="font-bold text-slate-400">UNRANKED</span> leaderboard.</p>
            </>,
            'At the end of each season pilots are redistributed across leagues again based on Global Rating.',
            'The natural way to move up is to get faster and climb higher in the Global Rating. However, situations arise where a pilot maintains their pace throughout the season but several faster pilots join their league — due to seat limits, that pilot may drop to a lower league. Conversely, if faster pilots fall out of the Global Rating, that pilot may take their spot in a higher league.',
            'This way, pilots are rewarded for speed, not for farming points over the month.',
        ],
        globalRatingTitle: 'Global Rating',
        globalRatingParagraphs: [
            'Global Rating is calculated once a week and shows how far behind the best pilots you are on average, as a percentage. It is based not on a single result but on all your results from the last 30 days. For each track, the system looks at the top-3 results and calculates their average time.',
            <div>
                <div className="bg-slate-900/60 border border-slate-700 p-4 space-y-1 text-sm font-mono">
                    <p className="text-slate-400 mb-2">Example:</p>
                    <p>Pilot 1 — 50s</p>
                    <p>Pilot 2 — 51s</p>
                    <p>Pilot 3 — 52s</p>
                </div>
                <p className="mt-3">Average time = <span className="text-emerald-400">51s</span>. Your gap is measured from this. If your time is 53s, you are approximately <span className="text-emerald-400">3.9%</span> slower.</p>
            </div>,
            'The system takes all your results from the last 30 days and calculates the average gap percentage. That number is your Global Rating GAP. The smaller the percentage, the higher your rank.',
            <div><p className="font-bold text-slate-100 border-l-2 border-emerald-400 pl-3">Why can first place have a negative GAP?</p><p className="mt-2 pl-3">Because the benchmark is the average time of the top-3. If a pilot flies faster than that average, their GAP turns negative — they are faster than the average top-3 level.</p></div>,
            <div><p className="font-bold text-slate-100 border-l-2 border-emerald-400 pl-3">Who qualifies?</p><p className="mt-2 pl-3">Only pilots who flew on at least 15 days out of the last 30 are included. This ensures the rating reflects consistent performance rather than one-off results.</p></div>,
        ],
        achievementsTitle: 'Achievements',
        achievementsText: 'The system has a growing set of achievements, awarded for reaching certain day streak milestones, finishing in first place, and more.',
        supportTitle: 'Support Us',
        supportText: <>You can support the project on our {link('https://patreon.com/FPVBattle', 'Patreon')} to help cover various costs (hosting, domain, coffee, etc.).</>,
        flyAway: "Let's fly! 🚀",
    },
};
