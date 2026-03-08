import { NavLink } from "react-router-dom";
import { LucideIcon } from "lucide-react";
import { ChevronRight } from "lucide-react";

interface SideMenuItemProps {
    to: string;
    icon: LucideIcon;
    label: string;
}

const SideMenuItem = ({ to, icon: Icon, label }: SideMenuItemProps) => {
    return (
        <NavLink
            to={to}
            className={({ isActive }) =>
                `flex items-center w-full transition-colors py-2 px-3 rounded
                 lg:justify-between
                 ${isActive
                    ? "text-emerald-400 bg-slate-700/50"
                    : "text-slate-200 hover:text-emerald-400"
                }`
            }
        >
            <span className="flex items-center gap-2">
                <Icon className="h-5 w-5 shrink-0" />
                <span>{label}</span>
            </span>
            <ChevronRight className="h-4 w-4 hidden lg:block" />
        </NavLink>
    );
};

export default SideMenuItem;
