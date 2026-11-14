import { NavLink } from "react-router-dom";
import { LucideIcon } from "lucide-react";
import { ChevronRight } from "lucide-react";

interface SideMenuItemProps {
    to: string;
    label: string;
    icon?: LucideIcon;
    /** Renders the item as a sub item of the entry above it. */
    nested?: boolean;
}

const SideMenuItem = ({ to, label, icon: Icon, nested = false }: SideMenuItemProps) => {
    return (
        <NavLink
            to={to}
            className={({ isActive }) =>
                `flex items-center w-full transition-colors py-2 px-3
                 lg:justify-between
                 ${nested ? "text-sm py-1 pl-7 lg:pl-10" : "justify-center text-xs lg:text-base"}
                 ${isActive
                    ? "text-emerald-400 bg-slate-700/50"
                    : "text-slate-200 hover:text-emerald-400"
                }`
            }
        >
            {/* Below lg the menu is a row of tabs, each with its icon above the label. */}
            <span className={`flex items-center gap-2 ${nested ? "" : "flex-col gap-1 text-center lg:flex-row lg:gap-2 lg:text-left"}`}>
                {Icon && <Icon className="h-5 w-5 shrink-0" />}
                <span>{label}</span>
            </span>
            {!nested && <ChevronRight className="h-4 w-4 hidden lg:block" />}
        </NavLink>
    );
};

export default SideMenuItem;
