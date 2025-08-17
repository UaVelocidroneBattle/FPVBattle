import { Link, NavLink, Outlet } from "react-router-dom"
import { ExternalLink } from "lucide-react";
import logo from "@/assets/logo.svg";

/**
 * Defines main layout that is applied to all top level pages
 */

const LayoutMain: React.FC = () => {
  return (
    <main className="min-h-screen bg-gradient-to-b from-slate-900 to-slate-800">
      <div className="max-w-[1800px] mx-auto px-4 py-8 sm:px-6 lg:px-8">
        <header className="flex flex-col items-center p-4 shadow-md mb-8">
          <h1 className="mb-4">
            <Link to="/" className="flex items-center">
              <img
                src={logo}
                alt="FPV Battle"
                className="h-10 md:h-14 w-auto"
              />
            </Link>
          </h1>

          <nav className="flex justify-center space-x-8 md:space-x-10">
            <NavLink
              to="/"
              className="text-slate-300 hover:text-emerald-400 transition-colors flex items-center space-x-2 hidden sm:block"
            >
              Dashboard
            </NavLink>
            <NavLink
              to="/rules"
              className="text-slate-300 hover:text-emerald-400 transition-colors flex items-center space-x-2"
            >
              Instructions
            </NavLink>
            <NavLink
              to="/statistics"
              className="text-slate-300 hover:text-emerald-400 transition-colors flex items-center space-x-2 hidden sm:block"
            >
              Statistics
            </NavLink>
            <a  href="https://t.me/fpv_velocidrone_ua"
                target="_blank"
                rel="noopener noreferrer"
                className="text-slate-300 hover:text-emerald-400 transition-colors inline-flex items-center"
                >
                Telegram <ExternalLink className="h-4 w-4 ml-2" />
            </a>
            <a
                href="https://discord.gg/FrpC2WV8Cw"
                target="_blank"
                rel="noopener noreferrer"
                className="text-slate-300 hover:text-emerald-400 transition-colors inline-flex items-center"
                >
                Discord <ExternalLink className="h-4 w-4 ml-2" />
            </a>

            <a
                href="https://patreon.com/FPVBattle"
                target="_blank"
                rel="noopener noreferrer"
                className="text-slate-300 hover:text-emerald-400 transition-colors inline-flex items-center"
                >
                Patreon <ExternalLink className="h-4 w-4 ml-2" />
            </a>
          </nav>
        </header>

        <Outlet />
      </div>
    </main>
  );
};

export default LayoutMain;