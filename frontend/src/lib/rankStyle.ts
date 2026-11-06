/** Gold, silver and bronze for the podium; muted for everyone else. */
export function rankStyle(rank: number): string {
    if (rank === 1) return "font-bold text-yellow-500";
    if (rank === 2) return "font-bold text-slate-400";
    if (rank === 3) return "font-bold text-amber-700";
    return "font-medium text-slate-500";
}
