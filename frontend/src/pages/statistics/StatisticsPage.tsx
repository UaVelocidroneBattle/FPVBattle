import { Outlet } from "react-router-dom";
import SideMenu from "./SideMenu";
import { bleedOnPhones } from "@/lib/layout";

const PageStatistics: React.FC = () => {
    return <>
        <div className="flex flex-col flex-1">
            <div className={`${bleedOnPhones} flex flex-col lg:flex-row gap-4 lg:gap-8 flex-1`}>
                <SideMenu />

                <main className="flex-1 bg-slate-800/50 backdrop-blur-sm border-y sm:border border-slate-700 px-4 py-5 sm:p-6">
                    <Outlet />
                </main>
            </div>
        </div>
    </>
}

export default PageStatistics;