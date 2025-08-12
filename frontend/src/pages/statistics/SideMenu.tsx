import { Calendar, Users } from "lucide-react";
import SideMenuItem from "../../components/ui/SideMenuItem";

const SideMenu = () => {
    return (
        <aside className="lg:w-64 bg-slate-800/50 backdrop-blur-sm border border-slate-700 rounded-lg overflow-hidden">
            <nav className="p-4">
                <ul className="space-y-2">
                    <li>
                        <SideMenuItem to="profile" icon={Calendar} label="Profile" />
                    </li>

                    <li>
                        <SideMenuItem to="pilots" icon={Users} label="Pilot Stats" />
                    </li>
                </ul>
            </nav>
        </aside>
    );
};

export default SideMenu;
