import { Link, NavLink, Outlet } from "react-router-dom"
import { ExternalLink } from "lucide-react";

/**
 * Defines main layout that is applied to all top level pages
 */

const LayoutMain: React.FC = () => {
    return <>
        <main className="min-h-screen bg-gradient-to-b from-slate-900 to-slate-800">
            <div className="max-w-[1800px] mx-auto px-4 py-8 sm:px-6 lg:px-8">
                <header className="mb-12 text-center">
                    <h1 className="text-4xl md:text-5xl font-bold bg-gradient-to-r from-emerald-400 to-cyan-400 bg-clip-text text-transparent mb-4">
                        <Link to={'/'}>UA Velocidrone Battle</Link>
                    </h1>
                    <nav className="flex justify-center space-x-8 md:space-x-10">
                        <NavLink to="/" className="text-slate-300 hover:text-emerald-400 transition-colors flex items-center space-x-2 hidden sm:block">
                            Dashboard
                        </NavLink>
                        <NavLink to="/rules" className="text-slate-300 hover:text-emerald-400 transition-colors flex items-center space-x-2">
                            Instructions
                        </NavLink>
                        <NavLink to={'/statistics'} className="text-slate-300 hover:text-emerald-400 transition-colors flex items-center space-x-2 hidden sm:block">
                            Statistics
                        </NavLink>
                        <Link to="https://t.me/fpv_velocidrone_ua" className="text-slate-300 hover:text-emerald-400 transition-colors inline-flex items-center">
                            Telegram Bot <ExternalLink className="h-4 w-4 ml-2" />
                        </Link>
                    </nav>
                </header>
                <Outlet></Outlet>
            </div>
        </main>
    </>
}

export default LayoutMain;