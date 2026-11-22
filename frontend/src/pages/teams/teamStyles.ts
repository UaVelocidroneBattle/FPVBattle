import type { TeamRole } from "@/api/client";
import { bleedOnPhones } from "@/lib/layout";

export const panelClass = `${bleedOnPhones} border-y sm:border border-slate-700 bg-slate-800/50 backdrop-blur-sm`;

export const sectionTitleClass = "text-sm font-semibold uppercase tracking-wide text-slate-400";

export const roleNameClass: Record<TeamRole, string> = {
    Member: "text-slate-300",
    Admin: "text-emerald-400",
    Owner: "text-amber-400",
};
