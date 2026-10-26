import { ReactNode, useEffect, useState } from "react";
import { Link, NavLink } from "react-router-dom";
import { LogOut, Menu, X } from "lucide-react";
import { getInitials } from "@/components/auth/getInitials";
import GoogleSignInButton from "@/components/auth/GoogleSignInButton";
import NotificationBell from "@/components/notifications/NotificationBell";
import { useCups } from "@/hooks/useCups";
import { useAuthStore } from "@/store/authStore";
import { SocialLinks, TeamsNavLink } from "./NavLinks";

/** Full-width rows, so the whole line is the tap target, with the current page marked by an accent bar. */
const rowClass = ({ isActive }: { isActive: boolean }) =>
    `flex min-h-12 items-center border-l-2 px-4 transition-colors ${isActive
        ? "border-emerald-400 bg-emerald-500/10 text-emerald-400"
        : "border-transparent text-slate-200 hover:bg-slate-800/60 hover:text-emerald-400"
    }`;

function MenuGroup({ label, children }: { label?: string; children: ReactNode }) {
    return (
        <div className="py-2">
            {label && (
                <p className="px-4 pb-1 pt-2 text-[11px] font-semibold uppercase tracking-wider text-slate-500">{label}</p>
            )}
            {children}
        </div>
    );
}

/** Who is signed in, first thing in the menu, with their notifications beside them. */
function AccountSummary({ onNavigate }: { onNavigate: () => void }) {
    const user = useAuthStore((state) => state.user);

    if (!user) {
        return (
            <div className="px-4 py-4">
                <GoogleSignInButton />
            </div>
        );
    }

    return (
        <div className="flex items-center gap-3 px-4 py-4">
            <Link to="/profile" onClick={onNavigate} className="group flex min-w-0 flex-1 items-center gap-3">
                <span className="flex h-10 w-10 flex-none items-center justify-center rounded-full bg-slate-700 text-sm font-semibold text-slate-200">
                    {getInitials(user.displayName, user.email)}
                </span>
                <span className="min-w-0">
                    <span className="block truncate text-sm font-semibold text-slate-200 transition-colors group-hover:text-emerald-400">
                        {user.displayName}
                    </span>
                    <span className="block text-xs text-slate-400">View profile</span>
                </span>
            </Link>
            <NotificationBell />
        </div>
    );
}

function MobileMenu() {
    const [open, setOpen] = useState(false);
    const close = () => setOpen(false);
    const user = useAuthStore((state) => state.user);
    const { cups } = useCups();

    // While open the menu owns the screen: the page behind stays put and Escape dismisses it.
    useEffect(() => {
        if (!open) return;

        const closeOnEscape = (event: KeyboardEvent) => {
            if (event.key === "Escape") setOpen(false);
        };
        const previousOverflow = document.body.style.overflow;

        document.addEventListener("keydown", closeOnEscape);
        document.body.style.overflow = "hidden";
        return () => {
            document.removeEventListener("keydown", closeOnEscape);
            document.body.style.overflow = previousOverflow;
        };
    }, [open]);

    const signOut = () => {
        close();
        useAuthStore.getState().logout();
    };

    return (
        <>
            <button
                className="lg:hidden relative z-50 text-slate-300 hover:text-emerald-400 transition-colors"
                onClick={() => setOpen(isOpen => !isOpen)}
                aria-label="Toggle menu"
                aria-expanded={open}
            >
                {open ? <X className="h-6 w-6" /> : <Menu className="h-6 w-6" />}
            </button>

            {open && (
                <>
                    <div
                        className="lg:hidden fixed inset-0 z-40 bg-slate-950/70 backdrop-blur-sm animate-in fade-in duration-200"
                        onClick={close}
                        aria-hidden
                    />

                    {/* Bleeds over the page gutter, so the sheet spans the whole screen. */}
                    <nav className="lg:hidden absolute -inset-x-4 top-full z-50 border-y border-slate-700 bg-slate-900 shadow-2xl animate-in fade-in slide-in-from-top-2 duration-200">
                        <AccountSummary onNavigate={close} />

                        <div className="border-t border-slate-800">
                            <MenuGroup>
                                <NavLink to="/" end className={rowClass} onClick={close}>Home</NavLink>
                            </MenuGroup>

                            <MenuGroup label="Compete">
                                {cups.map(cup => (
                                    <NavLink key={cup.id} to={`/${cup.id}`} className={rowClass} onClick={close}>{cup.name}</NavLink>
                                ))}
                            </MenuGroup>

                            <MenuGroup label="Explore">
                                <TeamsNavLink className={rowClass} onClick={close} />
                                <NavLink to="/guide" className={rowClass} onClick={close}>Guide</NavLink>
                                <NavLink to="/statistics" className={rowClass} onClick={close}>Statistics</NavLink>
                            </MenuGroup>
                        </div>

                        <div className="flex items-center justify-between border-t border-slate-800 px-4 py-4">
                            <SocialLinks iconClassName="h-5 w-5 text-slate-400" />
                            {user && (
                                <button
                                    onClick={signOut}
                                    className="flex items-center gap-1.5 text-sm text-slate-400 transition-colors hover:text-emerald-400"
                                >
                                    <LogOut className="h-4 w-4" />
                                    Sign out
                                </button>
                            )}
                        </div>
                    </nav>
                </>
            )}
        </>
    );
}

export default MobileMenu;
