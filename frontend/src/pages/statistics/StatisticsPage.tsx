import { Outlet } from "react-router-dom";
import SideMenu from "./SideMenu";

const PageStatistics: React.FC = () => {
    return <>
        <div className="min-h-screen ">
            <div className="max-w-[1800px] mx-auto px-4 py-8 sm:px-6 lg:px-8">

                <h2 className="text-3xl font-bold text-slate-200 mb-2">Statistics</h2>
                <p className="text-slate-400 mb-8">Analyze pilot performance and competition data</p>

                <div className="flex flex-col lg:flex-row gap-8">
                    <SideMenu></SideMenu>

                    <main className="flex-1 bg-slate-800/50 backdrop-blur-sm border border-slate-700 rounded-lg p-6">
                        <Outlet></Outlet>
                    </main>
                </div>
            </div>
        </div>
    </>
}

export default PageStatistics;