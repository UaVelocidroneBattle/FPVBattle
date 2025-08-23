import {Link, NavLink, Outlet} from "react-router-dom"
import {FaTelegramPlane} from "react-icons/fa";
import {SiDiscord} from "react-icons/si";
import {TbBrandPatreonFilled} from "react-icons/tb";
import logo from "/logo.svg";

/**
 * Defines main layout that is applied to all top level pages
 */

const LayoutMain: React.FC = () => {
    return (
        <main className="min-h-screen bg-gradient-to-b from-slate-900 to-slate-800 text-slate-200 pb-8 flex flex-col align-items-center">
            <div className="max-w-[1800px] w-full p-4 flex flex-col flex-1 mx-auto">
                <header className="flex flex-col items-center p-4 mb-4">
                    <h2 className="mb-4">
                        <Link to="/" className="flex items-center">
                            <img
                                src={logo}
                                alt="FPV Battle"
                                className="h-10 md:h-14 w-auto"
                            />
                        </Link>
                    </h2>

                    <nav className="flex justify-center gap-8 md:gap-100">
                        <NavLink
                            to="/"
                            className={({isActive}) =>
                                `transition-colors hidden sm:block ${isActive ? "text-emerald-400" : "text-slate-300 hover:text-emerald-400"}`
                            }
                        >
                            Dashboard
                        </NavLink>
                        <NavLink
                            to="/rules"
                            className={({isActive}) =>
                                `transition-colors ${isActive ? "text-emerald-400" : "text-slate-300 hover:text-emerald-400"}`
                            }
                        >
                            Instructions
                        </NavLink>
                        <NavLink
                            to="/statistics"
                            className={({isActive}) =>
                                `transition-colors ${isActive ? "text-emerald-400" : "text-slate-300 hover:text-emerald-400"}`
                            }
                        >
                            Statistics
                        </NavLink>
                        <div className="flex gap-8">
                            <a href="https://t.me/fpv_velocidrone_ua" title="Telegram" target="_blank" rel="noopener noreferrer">
                                <FaTelegramPlane className="h-6 w-6 text-slate-300 hover:text-emerald-400"/>
                            </a>
                            <a href="https://discord.gg/FrpC2WV8Cw" title="Discord" target="_blank" rel="noopener noreferrer">
                                <SiDiscord className="h-6 w-6 text-slate-300 hover:text-emerald-400"/>
                            </a>
                            <a href="https://patreon.com/FPVBattle" title="Patreon" target="_blank" rel="noopener noreferrer">
                                <TbBrandPatreonFilled className="h-6 w-6 text-slate-300 hover:text-emerald-400"/>
                            </a>
                        </div>
                    </nav>
                </header>
                <div className="flex-1 flex flex-col">
                    <Outlet/>
                </div>
            </div>
        </main>
    );
};

export default LayoutMain;