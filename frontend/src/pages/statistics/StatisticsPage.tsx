import { Outlet } from "react-router-dom";
import SideMenu from "./SideMenu";

const PageStatistics: React.FC = () => {
    return <>
        <div className="flex flex-col flex-1">
            <div className="flex flex-col lg:flex-row gap-8 flex-1">
                <SideMenu />

                <main className="flex-1 bg-slate-800/50 backdrop-blur-sm border border-slate-700 p-6">
                    <Outlet />
                </main>
            </div>
        </div>
    </>
}

export default PageStatistics;