import { NavLink, NavLinkProps } from "react-router-dom";
import { FaTelegramPlane } from "react-icons/fa";
import { SiDiscord, SiInstagram } from "react-icons/si";
import { TbBrandPatreonFilled } from "react-icons/tb";
import { useTeamsStore } from "@/store/teamsStore";

interface TeamsNavLinkProps {
    className: NavLinkProps["className"];
    onClick?: () => void;
}

export function TeamsNavLink({ className, onClick }: TeamsNavLinkProps) {
    const invitationCount = useTeamsStore((state) => state.overview?.invitations.length ?? 0);

    return (
        <NavLink to="/teams" className={className} onClick={onClick}>
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

export function SocialLinks({ iconClassName = "h-6 w-6 text-slate-300" }: { iconClassName?: string }) {
    return (
        <div className="flex gap-6">
            {socialLinks.map(({ href, title, Icon }) => (
                <a key={title} href={href} title={title} target="_blank" rel="noopener noreferrer">
                    <Icon className={`${iconClassName} hover:text-emerald-400 transition-colors`} />
                </a>
            ))}
        </div>
    );
}
