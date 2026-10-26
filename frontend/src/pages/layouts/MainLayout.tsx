import { useEffect } from "react";
import { Link, NavLink, Outlet } from "react-router-dom";
import logo from "/logo.svg";
import AuthControls from "@/components/auth/AuthControls";
import { useNotificationsPolling } from "@/components/notifications/useNotificationsPolling";
import { useCups } from "@/hooks/useCups";
import { usePageMeta } from "@/hooks/usePageMeta";
import { useAuthStore } from "@/store/authStore";
import { useProfileStore } from "@/store/profileStore";
import { useTeamsOverviewSync } from "@/hooks/useTeamsOverviewSync";
import MobileMenu from "./MobileMenu";
import { SocialLinks, TeamsNavLink } from "./NavLinks";

/**
 * Defines main layout that is applied to all top level pages
 */

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
    `transition-colors ${isActive ? "text-emerald-400" : "text-slate-300 hover:text-emerald-400"}`;

function LayoutMain() {
    const user = useAuthStore((state) => state.user);
    const { cups } = useCups();

    usePageMeta();
    useNotificationsPolling();
    useTeamsOverviewSync();

    useEffect(() => {
        const { restore } = useAuthStore.getState();
        restore();
    }, []);

    // The linked pilot is needed app-wide (e.g. to highlight the user's own
    // results in leaderboards), so it's fetched here rather than per-page.
    useEffect(() => {
        if (user) useProfileStore.getState().fetchProfile();
    }, [user]);

    return (
        <main className="min-h-screen bg-gradient-to-b from-slate-900 to-slate-800 text-slate-200 pb-8 flex flex-col">
            <div className="max-w-[1800px] w-full p-4 flex flex-col flex-1 mx-auto">
                <header className="py-3 mb-4 flex items-center justify-between relative">
                    <Link to="/" className="flex items-center">
                        <img src={logo} alt="FPV Battle" className="h-8 w-auto" />
                    </Link>

                    {/* Desktop nav */}
                    <nav className="hidden lg:flex items-center gap-8">
                        <NavLink to="/" end className={navLinkClass}>Home</NavLink>
                        {cups.map(cup => (
                            <NavLink key={cup.id} to={`/${cup.id}`} className={navLinkClass}>{cup.name}</NavLink>
                        ))}
                        <TeamsNavLink className={navLinkClass} />
                        <NavLink to="/guide" className={navLinkClass}>Guide</NavLink>
                        <NavLink to="/statistics" className={navLinkClass}>Statistics</NavLink>
                        <div className="w-px h-5 bg-slate-600" />
                        <SocialLinks />
                        <div className="flex items-center gap-4">
                            <div className="w-px h-5 bg-slate-600" />
                            <AuthControls />
                        </div>
                    </nav>

                    <MobileMenu />
                </header>

                <div className="flex-1 flex flex-col">
                    <Outlet />
                </div>
            </div>
        </main>
    );
}

export default LayoutMain;
