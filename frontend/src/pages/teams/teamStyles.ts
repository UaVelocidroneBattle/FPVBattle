import type { TeamRole } from "@/api/client";

export const panelClass = "border border-slate-700 bg-slate-800/50 backdrop-blur-sm";

export const sectionTitleClass = "text-sm font-semibold uppercase tracking-wide text-slate-400";

export const inputClass =
    "w-full border border-slate-600 bg-slate-900 px-3 py-2 text-sm placeholder:text-slate-500 focus:border-emerald-400 focus:outline-none";

export const secondaryButtonClass =
    "border border-slate-600 px-4 py-2 text-sm transition-colors hover:border-emerald-400 hover:text-emerald-400 disabled:pointer-events-none disabled:opacity-50";

export const primaryButtonClass =
    "border border-emerald-500 px-4 py-2 text-sm text-emerald-400 transition-colors hover:bg-emerald-500/10 disabled:pointer-events-none disabled:opacity-50";

export const compactSecondaryButtonClass =
    "border border-slate-600 px-3 py-1 text-sm transition-colors hover:border-emerald-400 hover:text-emerald-400 disabled:pointer-events-none disabled:opacity-50";

export const compactPrimaryButtonClass =
    "border border-emerald-500 px-3 py-1 text-sm text-emerald-400 transition-colors hover:bg-emerald-500/10 disabled:pointer-events-none disabled:opacity-50";

export const roleNameClass: Record<TeamRole, string> = {
    Member: "text-slate-300",
    Admin: "text-emerald-400",
    Owner: "text-amber-400",
};
