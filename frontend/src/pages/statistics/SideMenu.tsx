import { NavLink, useMatch } from "react-router-dom";
import { UserRound, ChartNoAxesCombined, Trophy, Flame, Users, Globe } from "lucide-react";
import SideMenuItem from "../../components/ui/SideMenuItem";
import { useCups } from "@/hooks/useCups";

/**
 * On phones the cups can't hang under "Global rating" without breaking the row
 * of tabs, so they get a row of their own beneath it while the rating is open.
 */
function MobileCupTabs() {
    const { cups } = useCups();

    return (
        <ul className="lg:hidden flex justify-center gap-2 border-t border-slate-700 p-2">
            {cups.map((cup) => (
                <li key={cup.id}>
                    <NavLink
                        to={`/global-rating/${cup.id}`}
                        className={({ isActive }) =>
                            `block rounded-full px-3 py-1 text-xs transition-colors ${isActive
                                ? "bg-emerald-500/15 text-emerald-400"
                                : "text-slate-300 hover:text-emerald-400"
                            }`
                        }
                    >
                        {cup.name}
                    </NavLink>
                </li>
            ))}
        </ul>
    );
}

const SideMenu = () => {
    const { cups } = useCups();
    const isRatingOpen = useMatch("/global-rating/*") !== null;

    return (
        <aside className="lg:w-64 bg-slate-800/50 backdrop-blur-sm border-y sm:border border-slate-700 overflow-hidden">
            <nav className="p-2 lg:p-4">
                <ul className="flex lg:flex-col gap-1 lg:gap-2">
                    {/* Absolute paths: the rating sits outside /statistics. */}
                    <li className="flex-1 lg:flex-none">
                        <SideMenuItem to="/global-rating" icon={Trophy} label="Global rating" />
                        <ul className="hidden lg:block">
                            {cups.map((cup) => (
                                <li key={cup.id}>
                                    <SideMenuItem to={`/global-rating/${cup.id}`} label={cup.name} nested />
                                </li>
                            ))}
                        </ul>
                    </li>
                    <li className="flex-1 lg:flex-none">
                        <SideMenuItem to="/pilot" icon={UserRound} label="Pilot profile" />
                    </li>
                    <li className="hidden sm:flex flex-1 lg:flex-none">
                        <SideMenuItem to="/statistics/pilots" icon={Users} label="Pilot numbers" />
                    </li>
                    <li className="flex-1 lg:flex-none">
                        <SideMenuItem to="/statistics/daystreaks" icon={Flame} label="Day Streaks" />
                    </li>
                    <li className="hidden sm:flex flex-1 lg:flex-none">
                        <SideMenuItem to="/statistics/performance" icon={ChartNoAxesCombined} label="Compare pilots" />
                    </li>
                    <li className="hidden sm:flex flex-1 lg:flex-none">
                        <SideMenuItem to="/statistics/countries" icon={Globe} label="Countries" />
                    </li>
                </ul>
            </nav>
            {isRatingOpen && <MobileCupTabs />}
        </aside>
    );
};

export default SideMenu;
