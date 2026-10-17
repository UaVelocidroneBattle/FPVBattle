import { useEffect, useState } from "react";
import { Link, NavLink, Outlet } from "react-router-dom";
import { Menu, X } from "lucide-react";
import { FaTelegramPlane } from "react-icons/fa";
import { SiDiscord, SiInstagram } from "react-icons/si";
import { TbBrandPatreonFilled } from "react-icons/tb";
import logo from "/logo.svg";
import AuthControls from "@/components/auth/AuthControls";
import { useNotificationsPolling } from "@/components/notifications/useNotificationsPolling";
import { useCups } from "@/hooks/useCups";
import { usePageMeta } from "@/hooks/usePageMeta";
import { useAuthStore } from "@/store/authStore";
import { useProfileStore } from "@/store/profileStore";
import { useTeamsStore } from "@/store/teamsStore";
import { useTeamsOverviewSync } from "@/hooks/useTeamsOverviewSync";

/**
 * Defines main layout that is applied to all top level pages
 */

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
    `transition-colors ${isActive ? "text-emerald-400" : "text-slate-300 hover:text-emerald-400"}`;

function TeamsNavLink({ onClick }: { onClick?: () => void }) {
    const invitationCount = useTeamsStore((state) => state.overview?.invitations.length ?? 0);

    return (
        <NavLink to="/teams" className={navLinkClass} onClick={onClick}>
            <span className="relative">
                Teams
                {invitationCount > 0 && (
                    <span
                        className="absolute -right-4 -top-2 flex h-4 min-w-4 items-center justify-center rounded-full bg-emerald-500 px-1 text-[10px] font-semibold text-slate-900"
                        aria-label={`${invitationCount} team invitations`}
                    >
                        {invitationCount > 9 ? "9+" : invitationCount}
                    </span>
                )}
            </span>
        </NavLink>
    );
}

const socialLinks = [
    { href: "https://t.me/fpv_velocidrone_ua", title: "Telegram", Icon: FaTelegramPlane },
    { href: "https://discord.gg/FrpC2WV8Cw", title: "Discord", Icon: SiDiscord },
    { href: "https://patreon.com/FPVBattle", title: "Patreon", Icon: TbBrandPatreonFilled },
    { href: "https://www.instagram.com/fpv_battle/", title: "Instagram", Icon: SiInstagram },
];

function SocialLinks() {
    return (
        <div className="flex gap-6">
            {socialLinks.map(({ href, title, Icon }) => (
                <a key={title} href={href} title={title} target="_blank" rel="noopener noreferrer">
                    <Icon className="h-6 w-6 text-slate-300 hover:text-emerald-400 transition-colors" />
                </a>
            ))}
        </div>
    );
}

function LayoutMain() {
    const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
    const closeMobileMenu = () => setMobileMenuOpen(false);
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
                        <TeamsNavLink />
                        <NavLink to="/guide" className={navLinkClass}>Guide</NavLink>
                        <NavLink to="/statistics" className={navLinkClass}>Statistics</NavLink>
                        <div className="w-px h-5 bg-slate-600" />
                        <SocialLinks />
                        <div className="flex items-center gap-4">
                            <div className="w-px h-5 bg-slate-600" />
                            <AuthControls />
                        </div>
                    </nav>

                    {/* Mobile hamburger */}
                    <button
                        className="lg:hidden text-slate-300 hover:text-emerald-400 transition-colors"
                        onClick={() => setMobileMenuOpen(open => !open)}
                        aria-label="Toggle menu"
                    >
                        {mobileMenuOpen ? <X className="h-6 w-6" /> : <Menu className="h-6 w-6" />}
                    </button>

                    {/* Mobile dropdown */}
                    {mobileMenuOpen && (
                        <nav className="lg:hidden absolute top-16 left-0 right-0 z-10 bg-slate-900 border-t border-slate-700 px-6 py-4 flex flex-col gap-5">
                            <NavLink to="/" end className={navLinkClass} onClick={closeMobileMenu}>Home</NavLink>
                            {cups.map(cup => (
                                <NavLink key={cup.id} to={`/${cup.id}`} className={navLinkClass} onClick={closeMobileMenu}>{cup.name}</NavLink>
                            ))}
                            <TeamsNavLink onClick={closeMobileMenu} />
                            <NavLink to="/guide" className={navLinkClass} onClick={closeMobileMenu}>Guide</NavLink>
                            <NavLink to="/statistics" className={navLinkClass} onClick={closeMobileMenu}>Statistics</NavLink>
                            <SocialLinks />
                            <AuthControls />
                        </nav>
                    )}
                </header>

                <div className="flex-1 flex flex-col">
                    <Outlet />
                </div>
            </div>
        </main>
    );
}

export default LayoutMain;
