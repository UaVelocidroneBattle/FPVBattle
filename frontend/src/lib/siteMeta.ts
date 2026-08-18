/**
 * Single source of truth for the public site URL and per-route page metadata.
 *
 * Changing domain? Update SITE_URL below, then the same host in
 * `index.html` (canonical, og:url, og:image, JSON-LD), `public/robots.txt`
 * and `public/sitemap.xml` — those are static files the app never touches.
 */
export const SITE_URL = 'https://fpv-battle.fun';

/** Appended to every title except the landing page, so the brand is in each search result. */
const BRAND_SUFFIX = ' | FPV Battle';

export interface PageMeta {
    title: string;
    description: string;
    /** Keeps per-user or thin pages out of search results. */
    noIndex?: boolean;
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
const PAGE_META: Record<string, PageMeta> = {
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

    '/statistics': {
        title: `Statistics — Velocidrone Pilot Rankings${BRAND_SUFFIX}`,
        description:
            'Global rating, day streak standings, track history and pilot statistics from the FPV Battle daily Velocidrone competition.',
    },
    '/global-rating': {
        title: `Global Rating Leaderboard${BRAND_SUFFIX}`,
        description:
            'The full FPV Battle global rating leaderboard — every Velocidrone pilot ranked by their results across the daily competitions.',
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
    },
    '/statistics/pilots': {
        title: `Pilot Numbers${BRAND_SUFFIX}`,
        description:
            'How the FPV Battle pilot community is growing — total Velocidrone pilots over time, new pilots each month and daily participation.',
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
