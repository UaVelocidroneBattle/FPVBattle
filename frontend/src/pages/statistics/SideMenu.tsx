import { UserRound, ChartNoAxesCombined, Trophy } from "lucide-react";
import SideMenuItem from "../../components/ui/SideMenuItem";

const SideMenu = () => {
    return (
        <aside className="lg:w-64 bg-slate-800/50 backdrop-blur-sm border border-slate-700 rounded-lg overflow-hidden">
            <nav className="p-4">
                <ul className="space-y-2">
                    <li>
                        <SideMenuItem to="global-rating" icon={Trophy} label="Global rating" />
                    </li>

                    <li>
                        <SideMenuItem to="profile" icon={UserRound} label="Profile" />
                    </li>

                    <li className="hidden sm:block">
                        <SideMenuItem to="pilots" icon={ChartNoAxesCombined} label="Pilot stats" />
                    </li>
                </ul>
            </nav>
        </aside>
    );
};

export default SideMenu;
