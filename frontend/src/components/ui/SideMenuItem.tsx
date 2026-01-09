import { Link } from "react-router-dom";
import { LucideIcon } from "lucide-react";
import { ChevronRight } from "lucide-react";

interface SideMenuItemProps {
    to: string;
    icon: LucideIcon;
    label: string;
}

const SideMenuItem = ({ to, icon: Icon, label }: SideMenuItemProps) => {
    return (
        <Link to={to}
            className="flex items-center justify-between w-full text-left text-slate-200 hover:text-emerald-400 transition-colors py-2 px-3 rounded"
        >
            <span className="flex items-center">
                <Icon className="h-5 w-5 mr-2" />
                <span>{label}</span>
            </span>
            <ChevronRight className="h-4 w-4" />
        </Link>
    );
};

export default SideMenuItem;
