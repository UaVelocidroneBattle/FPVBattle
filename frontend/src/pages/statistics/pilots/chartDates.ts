/**
 * The API labels its points `yyyy-MM-dd` and its bars `yyyy-MM`.
 *
 * `new Date('2026-08-17')` reads those as UTC midnight, which renders as the day
 * before anywhere west of Greenwich, so the labels are taken apart by hand and
 * rebuilt in local time. A chart of daily counts that is a day out is worse than
 * one that fails outright, because nothing about it looks wrong.
 */

/** Parses a `yyyy-MM-dd` label as local midnight. */
export function parseDay(day: string): Date {
    const [year, month, date] = day.split('-').map(Number);

    return new Date(year, month - 1, date);
}

/** Parses a `yyyy-MM` label as local midnight on the first of the month. */
export function parseMonth(month: string): Date {
    const [year, monthNumber] = month.split('-').map(Number);

    return new Date(year, monthNumber - 1, 1);
}

/** `17.08.2026` — the European format used across the site. */
export function formatDay(day: string): string {
    const [year, month, date] = day.split('-');

    return `${date}.${month}.${year}`;
}

/** `Aug 26` — short enough to sit under a bar without turning sideways. */
export function formatMonthShort(month: string): string {
    return parseMonth(month).toLocaleDateString('en-GB', { month: 'short', year: '2-digit' });
}

/** `August 2026` — for tooltips, where there is room to spell it out. */
export function formatMonthLong(month: string): string {
    return parseMonth(month).toLocaleDateString('en-GB', { month: 'long', year: 'numeric' });
}

/**
 * How often a daily chart should label its horizontal axis, as a nivo tick interval.
 *
 * Keeps every range to roughly a dozen labels: a month of weekly ticks is readable,
 * three years of monthly ones is a smear.
 */
export function dayTickInterval(days: number): string {
    if (days > 730) return 'every 6 months';
    if (days > 400) return 'every 3 months';
    if (days > 200) return 'every 2 months';
    if (days > 60) return 'every 1 month';

    return 'every 1 week';
}
