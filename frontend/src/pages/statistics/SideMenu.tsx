import { UserRound, ChartNoAxesCombined, Trophy } from "lucide-react";
import SideMenuItem from "../../components/ui/SideMenuItem";

const SideMenu = () => {
    return (
        <aside className="lg:w-64 bg-slate-800/50 backdrop-blur-sm border border-slate-700 overflow-hidden">
            <nav className="p-2 lg:p-4">
                <ul className="flex lg:flex-col gap-1 lg:gap-2">
                    <li className="flex-1 lg:flex-none">
                        <SideMenuItem to="global-rating" icon={Trophy} label="Global rating" />
                    </li>
                    <li className="flex-1 lg:flex-none">
                        <SideMenuItem to="profile" icon={UserRound} label="Profile" />
                    </li>
                    <li className="hidden sm:flex flex-1 lg:flex-none">
                        <SideMenuItem to="pilots" icon={ChartNoAxesCombined} label="Pilot stats" />
                    </li>
                </ul>
            </nav>
        </aside>
    );
};

export default SideMenu;
