import { useEffect, useRef, useState } from "react";
import { Link } from "react-router-dom";
import { useAuthStore } from "@/store/authStore";
import GoogleSignInButton from "./GoogleSignInButton";

/**
 * Header widget: Google sign-in button when signed out,
 * initials avatar with a dropdown menu (identity, Profile, Sign out) when signed in.
 */

function getInitials(displayName: string, email: string) {
    const words = displayName.trim().split(/\s+/).filter(Boolean);
    const initials = words.slice(0, 2).map((word) => word[0]).join("");
    return (initials || email[0] || "?").toUpperCase();
}
function AuthControls() {
    const user = useAuthStore((state) => state.user);
    const [menuOpen, setMenuOpen] = useState(false);
    const containerRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if (!menuOpen) return;

        const closeOnOutsideClick = (event: MouseEvent) => {
            if (!containerRef.current?.contains(event.target as Node)) {
                setMenuOpen(false);
            }
        };
        document.addEventListener("mousedown", closeOnOutsideClick);
        return () => document.removeEventListener("mousedown", closeOnOutsideClick);
    }, [menuOpen]);

    if (!user) return <GoogleSignInButton />;

    const signOut = () => {
        setMenuOpen(false);
        useAuthStore.getState().logout();
    };

    return (
        <div ref={containerRef} className="relative flex items-center">
            <button
                onClick={() => setMenuOpen(open => !open)}
                aria-label="User menu"
                className="flex h-8 w-8 items-center justify-center rounded-full bg-slate-700 text-xs font-semibold text-slate-200 transition-colors hover:bg-slate-600 hover:text-emerald-400"
            >
                {getInitials(user.displayName, user.email)}
            </button>

            {menuOpen && (
                <div className="absolute right-0 top-full z-20 mt-2 w-56 border border-slate-700 bg-slate-900 py-1 shadow-lg">
                    <div className="flex items-center gap-3 border-b border-slate-700 px-4 py-3">
                        <span className="flex h-8 w-8 flex-none items-center justify-center rounded-full bg-slate-700 text-xs font-semibold text-slate-200">
                            {getInitials(user.displayName, user.email)}
                        </span>
                        <div className="min-w-0">
                            <p className="truncate text-sm font-semibold text-slate-200">{user.displayName}</p>
                            <p className="truncate text-xs text-slate-400">{user.email}</p>
                        </div>
                    </div>
                    <Link
                        to="/profile"
                        onClick={() => setMenuOpen(false)}
                        className="block w-full px-4 py-2 text-left text-sm text-slate-300 transition-colors hover:text-emerald-400"
                    >
                        Profile
                    </Link>
                    <button
                        onClick={signOut}
                        className="block w-full px-4 py-2 text-left text-sm text-slate-300 transition-colors hover:text-emerald-400"
                    >
                        Sign out
                    </button>
                </div>
            )}
        </div>
    );
}

export default AuthControls;
