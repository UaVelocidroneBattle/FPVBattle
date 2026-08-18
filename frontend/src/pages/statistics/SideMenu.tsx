import { UserRound, ChartNoAxesCombined, Trophy, Flame, Users } from "lucide-react";
import SideMenuItem from "../../components/ui/SideMenuItem";
import { useCups } from "@/hooks/useCups";

const SideMenu = () => {
    const { cups } = useCups();

    return (
        <aside className="lg:w-64 bg-slate-800/50 backdrop-blur-sm border border-slate-700 overflow-hidden">
            <nav className="p-2 lg:p-4">
                <ul className="flex lg:flex-col gap-1 lg:gap-2">
                    {/* Absolute paths: the rating sits outside /statistics. */}
                    <li className="flex-1 lg:flex-none">
                        <SideMenuItem to="/global-rating" icon={Trophy} label="Global rating" />
                        <ul>
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
                    <li className="flex-1 lg:flex-none">
                        <SideMenuItem to="/statistics/daystreaks" icon={Flame} label="Day Streaks" />
                    </li>
                    <li className="hidden sm:flex flex-1 lg:flex-none">
                        <SideMenuItem to="/statistics/pilots" icon={Users} label="Pilot numbers" />
                    </li>
                    <li className="hidden sm:flex flex-1 lg:flex-none">
                        <SideMenuItem to="/statistics/performance" icon={ChartNoAxesCombined} label="Compare pilots" />
                    </li>
                </ul>
            </nav>
        </aside>
    );
};

export default SideMenu;
