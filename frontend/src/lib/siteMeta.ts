/**
 * Single source of truth for the public site URL and per-route page metadata.
 *
 * Changing domain? Update SITE_URL below, then the same host in
 * `index.html` (og:image, JSON-LD) and `public/robots.txt` — static files
 * the app never touches.
 */
export const SITE_URL = 'https://fpv-battle.fun';

/** Appended to every title except the landing page, so the brand is in each search result. */
const BRAND_SUFFIX = ' | FPV Battle';

export interface PageMeta {
    title: string;
    description: string;
    /** Keeps per-user or thin pages out of search results. */
    noIndex?: boolean;
    /** Only a fallback for its child routes, so it is left out of the sitemap. */
    parentOnly?: boolean;
}

const LANDING_META: PageMeta = {
    title: 'FPV Battle — Daily Velocidrone Competitions & Leaderboards',
    description:
        'FPV Battle runs daily Velocidrone racing competitions. A new track every day, live leaderboards, global rating, day streaks and achievements for sim pilots.',
};

/**
 * Keyed by exact pathname. A path with no entry falls back to the closest
 * parent entry, then to the landing page metadata.
 */
export const PAGE_META: Record<string, PageMeta> = {
    '/': LANDING_META,

    '/open-class': {
        title: `Open Class — Today's Track & Leaderboard${BRAND_SUFFIX}`,
        description:
            "Today's Open Class track on Velocidrone, with the live leaderboard, lap times and points for every pilot in the daily FPV Battle competition.",
    },
    '/whoop-class': {
        title: `Whoop Class — Today's Track & Leaderboard${BRAND_SUFFIX}`,
        description:
            "Today's Whoop Class track on Velocidrone, with the live leaderboard, lap times and points for every pilot in the daily FPV Battle competition.",
    },

    '/profile': {
        title: `Your Profile${BRAND_SUFFIX}`,
        description: 'Manage your FPV Battle account and the Velocidrone pilot linked to it.',
        noIndex: true,
    },

    '/guide': {
        title: `Guide — How FPV Battle Works${BRAND_SUFFIX}`,
        description:
            'How the daily Velocidrone competition works: joining, scoring, global rating, leagues, day streaks, freezies and achievements.',
        parentOnly: true,
    },
    '/guide/getting-started': {
        title: `Getting Started — Join the Daily Velocidrone Race${BRAND_SUFFIX}`,
        description:
            'Everything you need to start flying the FPV Battle daily competition in Velocidrone: what you need, how to join and how to get your first result counted.',
    },
    '/guide/how-it-works': {
        title: `How It Works — Daily Tracks & Scoring${BRAND_SUFFIX}`,
        description:
            'A new Velocidrone track every day, results tracked through the day, and final standings with points awarded at midnight. Here is how FPV Battle scoring works.',
    },
    '/guide/global-rating': {
        title: `Global Rating Explained${BRAND_SUFFIX}`,
        description:
            'How the FPV Battle global rating is calculated from your daily Velocidrone results, and what moves you up and down the rankings.',
    },
    '/guide/leagues': {
        title: `Leagues Explained${BRAND_SUFFIX}`,
        description:
            'How FPV Battle leagues group pilots by skill so you race against opponents at your own level, and how promotion and relegation work.',
    },
    '/guide/day-streak': {
        title: `Day Streak Explained${BRAND_SUFFIX}`,
        description:
            'A day streak counts the consecutive days you have flown the daily Velocidrone track without missing one. Here is how it builds, and how it breaks.',
    },
    '/guide/freeze': {
        title: `Day Streak Freeze (Freezies) Explained${BRAND_SUFFIX}`,
        description:
            'Freezies save your FPV Battle day streak on the days you cannot fly. How to earn them, how many you can hold and when they are spent.',
    },
    '/guide/quad-of-the-day': {
        title: `Quad of the Day Explained${BRAND_SUFFIX}`,
        description:
            'What the Quad of the Day is in FPV Battle, how the daily quad is chosen and how it changes the way you fly the track.',
    },
    '/guide/achievements': {
        title: `Achievements List${BRAND_SUFFIX}`,
        description:
            'Every achievement you can unlock in FPV Battle, from day streak milestones to race placements in the daily Velocidrone competition.',
    },
    '/guide/support': {
        title: `Support FPV Battle${BRAND_SUFFIX}`,
        description:
            'How to support FPV Battle on Patreon and help keep the daily Velocidrone competition running.',
    },

    '/uk/guide': {
        title: `Гайд — як працює FPV Battle${BRAND_SUFFIX}`,
        description:
            'Як працюють щоденні змагання у Velocidrone: участь, нарахування балів, global rating, ліги, day streak, freezies та achievements.',
        parentOnly: true,
    },
    '/uk/guide/getting-started': {
        title: `Як почати — щоденні перегони у Velocidrone${BRAND_SUFFIX}`,
        description:
            'Усе, що потрібно, щоб почати літати щоденні змагання FPV Battle у Velocidrone: що знадобиться, як долучитися і як зарахувати свій перший результат.',
    },
    '/uk/guide/how-it-works': {
        title: `Як це працює — щоденні траси та бали${BRAND_SUFFIX}`,
        description:
            'Щодня нова траса у Velocidrone, результати відстежуються впродовж дня, а опівночі — фінальна таблиця та бали. Ось як нараховуються бали у FPV Battle.',
    },
    '/uk/guide/global-rating': {
        title: `Як працює Global Rating${BRAND_SUFFIX}`,
        description:
            'Як global rating FPV Battle рахується з ваших щоденних результатів у Velocidrone і що піднімає чи опускає вас у рейтингу.',
    },
    '/uk/guide/leagues': {
        title: `Як працюють ліги${BRAND_SUFFIX}`,
        description:
            'Як ліги FPV Battle групують пілотів за рівнем, щоб ви змагалися із суперниками свого рівня, і як працюють підвищення та пониження.',
    },
    '/uk/guide/day-streak': {
        title: `Як працює Day Streak${BRAND_SUFFIX}`,
        description:
            'Day streak — це кількість днів поспіль, коли ви літали щоденну трасу у Velocidrone без пропусків. Ось як він росте і як переривається.',
    },
    '/uk/guide/freeze': {
        title: `Day Streak Freeze (freezies) — як це працює${BRAND_SUFFIX}`,
        description:
            'Freezies рятують ваш day streak у FPV Battle в дні, коли не вдається полетіти. Як їх отримати, скільки можна мати і коли вони витрачаються.',
    },
    '/uk/guide/quad-of-the-day': {
        title: `Квад дня — як це працює${BRAND_SUFFIX}`,
        description:
            'Що таке квад дня у FPV Battle, як його обирають і як він змінює те, як ви літаєте трасу.',
    },
    '/uk/guide/achievements': {
        title: `Список Achievements${BRAND_SUFFIX}`,
        description:
            'Усі achievements, які можна отримати у FPV Battle: від рубежів day streak до призових місць у щоденних змаганнях у Velocidrone.',
    },
    '/uk/guide/support': {
        title: `Підтримати FPV Battle${BRAND_SUFFIX}`,
        description:
            'Як підтримати FPV Battle на Patreon і допомогти щоденним змаганням у Velocidrone працювати далі.',
    },

    '/statistics': {
        title: `Statistics — Velocidrone Pilot Rankings${BRAND_SUFFIX}`,
        description:
            'Global rating, day streak standings, track history and pilot statistics from the FPV Battle daily Velocidrone competition.',
        parentOnly: true,
    },
    '/global-rating': {
        title: `Global Rating Leaderboard${BRAND_SUFFIX}`,
        description:
            'The full FPV Battle global rating leaderboard — every Velocidrone pilot ranked by their results across the daily competitions.',
        parentOnly: true,
    },
    '/global-rating/open-class': {
        title: `Open Class Global Rating${BRAND_SUFFIX}`,
        description:
            'Every Open Class Velocidrone pilot ranked by their results across the FPV Battle daily competitions.',
    },
    '/global-rating/whoop-class': {
        title: `Whoop Class Global Rating${BRAND_SUFFIX}`,
        description:
            'Every Whoop Class Velocidrone pilot ranked by their results across the FPV Battle daily competitions.',
    },
    '/statistics/daystreaks': {
        title: `Day Streak Leaderboard${BRAND_SUFFIX}`,
        description:
            'Which Velocidrone pilots have flown the most consecutive days. The full FPV Battle day streak standings, current and all-time.',
    },
    '/statistics/tracks': {
        title: `Track History${BRAND_SUFFIX}`,
        description:
            'Every Velocidrone track used in the FPV Battle daily competition, with the results and fastest times recorded on each one.',
        noIndex: true,
    },
    '/statistics/pilots': {
        title: `Pilot Numbers${BRAND_SUFFIX}`,
        description:
            'How the FPV Battle pilot community is growing — total Velocidrone pilots over time, new pilots each month and daily participation.',
    },
    '/statistics/countries': {
        title: `Pilots by Country${BRAND_SUFFIX}`,
        description:
            'Where FPV Battle pilots fly from — every country in the daily Velocidrone competition and the pilots representing it.',
    },
    '/statistics/performance': {
        title: `Pilot Comparison${BRAND_SUFFIX}`,
        description:
            'Compare Velocidrone pilots head to head across their FPV Battle results, ratings and day streaks.',
    },
    '/pilot': {
        title: `Pilot Profile${BRAND_SUFFIX}`,
        description:
            'Race history, day streak, achievements and results heatmap for a Velocidrone pilot competing in FPV Battle.',
        parentOnly: true,
    },
};

/** Strips the trailing slash so '/guide/' and '/guide' resolve identically. */
function normalizePath(pathname: string): string {
    return pathname.length > 1 && pathname.endsWith('/') ? pathname.slice(0, -1) : pathname;
}

/**
 * Finds the deepest configured ancestor of `path`, so dynamic routes such as
 * '/pilot/Jack' inherit their parent's metadata.
 */
function findAncestorMeta(path: string): PageMeta | undefined {
    const ancestors = Object.keys(PAGE_META)
        .filter(candidate => path.startsWith(`${candidate}/`))
        .sort((a, b) => b.length - a.length);

    return ancestors.length > 0 ? PAGE_META[ancestors[0]] : undefined;
}

export function resolvePageMeta(pathname: string): PageMeta {
    const path = normalizePath(pathname);
    return PAGE_META[path] ?? findAncestorMeta(path) ?? LANDING_META;
}

export type PageLanguage = 'en' | 'uk';

/** Ukrainian translations mirror the English routes under this prefix, e.g. /uk/guide/leagues. */
export const UKRAINIAN_PREFIX = '/uk';

export function pageLanguage(pathname: string): PageLanguage {
    const path = normalizePath(pathname);
    return path === UKRAINIAN_PREFIX || path.startsWith(`${UKRAINIAN_PREFIX}/`) ? 'uk' : 'en';
}

export interface PageAlternate {
    hreflang: PageLanguage | 'x-default';
    path: string;
}

/** Every language version of a translated page, for hreflang links. Empty for English-only pages. */
export function pageAlternates(pathname: string): PageAlternate[] {
    const path = normalizePath(pathname);
    const englishPath = pageLanguage(path) === 'uk' ? path.slice(UKRAINIAN_PREFIX.length) : path;
    const ukrainianPath = `${UKRAINIAN_PREFIX}${englishPath}`;

    if (!PAGE_META[englishPath] || !PAGE_META[ukrainianPath]) {
        return [];
    }

    return [
        { hreflang: 'en', path: englishPath },
        { hreflang: 'uk', path: ukrainianPath },
        { hreflang: 'x-default', path: englishPath },
    ];
}
