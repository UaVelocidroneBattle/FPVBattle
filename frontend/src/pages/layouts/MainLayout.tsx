import { useState } from "react";
import { Link, NavLink, Outlet } from "react-router-dom";
import { Menu, X } from "lucide-react";
import { FaTelegramPlane } from "react-icons/fa";
import { SiDiscord, SiInstagram } from "react-icons/si";
import { TbBrandPatreonFilled } from "react-icons/tb";
import logo from "/logo.svg";

/**
 * Defines main layout that is applied to all top level pages
 */

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
    `transition-colors ${isActive ? "text-emerald-400" : "text-slate-300 hover:text-emerald-400"}`;

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

    return (
        <main className="min-h-screen bg-gradient-to-b from-slate-900 to-slate-800 text-slate-200 pb-8 flex flex-col">
            <div className="max-w-[1800px] w-full p-4 flex flex-col flex-1 mx-auto">
                <header className="p-4 mb-4">
                    {/* Top row: logo + hamburger (mobile) / centered logo (desktop) */}
                    <div className="flex items-center justify-between sm:justify-center">
                        <Link to="/" className="flex items-center">
                            <img src={logo} alt="FPV Battle" className="h-10 md:h-14 w-auto" />
                        </Link>

                        <button
                            className="sm:hidden text-slate-300 hover:text-emerald-400 transition-colors"
                            onClick={() => setMobileMenuOpen(open => !open)}
                            aria-label="Toggle menu"
                        >
                            {mobileMenuOpen ? <X className="h-6 w-6" /> : <Menu className="h-6 w-6" />}
                        </button>
                    </div>

                    {/* Desktop nav */}
                    <nav className="hidden sm:flex justify-center items-center gap-8 mt-4">
                        <NavLink to="/" end className={navLinkClass}>Open Class</NavLink>
                        <NavLink to="/whoop" className={navLinkClass}>Whoop Class</NavLink>
                        <NavLink to="/rules" className={navLinkClass}>Instructions</NavLink>
                        <NavLink to="/statistics" className={navLinkClass}>Statistics</NavLink>
                        <SocialLinks />
                    </nav>

                    {/* Mobile dropdown */}
                    {mobileMenuOpen && (
                        <nav className="sm:hidden mt-4 pt-4 border-t border-slate-700 flex flex-col gap-5">
                            <NavLink to="/" end className={navLinkClass} onClick={closeMobileMenu}>Open Class</NavLink>
                            <NavLink to="/whoop" className={navLinkClass} onClick={closeMobileMenu}>Whoop Class</NavLink>
                            <NavLink to="/rules" className={navLinkClass} onClick={closeMobileMenu}>Instructions</NavLink>
                            <NavLink to="/statistics" className={navLinkClass} onClick={closeMobileMenu}>Statistics</NavLink>
                            <SocialLinks />
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
