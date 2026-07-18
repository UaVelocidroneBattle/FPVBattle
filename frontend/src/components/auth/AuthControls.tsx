import { useEffect, useRef, useState } from "react";
import { Link } from "react-router-dom";
import { FaUserCircle } from "react-icons/fa";
import { useAuthStore } from "@/store/authStore";
import GoogleSignInButton from "./GoogleSignInButton";

/**
 * Header widget: Google sign-in button when signed out,
 * avatar with a dropdown menu (Profile, Sign out) when signed in.
 */
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
                title={user.displayName}
                aria-label="User menu"
                className="flex items-center"
            >
                <FaUserCircle className="h-6 w-6 text-slate-300 hover:text-emerald-400 transition-colors" />
            </button>

            {menuOpen && (
                <div className="absolute right-0 top-full z-20 mt-2 w-36 rounded-lg border border-slate-700 bg-slate-900 py-1 shadow-lg">
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
