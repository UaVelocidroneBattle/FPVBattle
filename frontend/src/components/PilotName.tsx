import { Link } from "react-router-dom";

interface PilotNameProps {
    name: string;
    /** Tag of the pilot's team, shown in front of the name. */
    teamTag?: string | null;
    className?: string;
}

function PilotName({ name, teamTag, className = "" }: PilotNameProps) {
    const link = (
        <Link
            to={`/pilot/${encodeURIComponent(name)}`}
            className={`hover:text-emerald-400 transition-colors cursor-pointer ${className}`}
            title={`View ${name}'s profile`}
        >
            {name}
        </Link>
    );

    if (!teamTag) return link;

    return (
        <span className="flex min-w-0 items-baseline gap-2">
            <span className="shrink-0 text-xs font-semibold tracking-wide text-slate-500">[{teamTag}]</span>
            {link}
        </span>
    );
}

export default PilotName;
