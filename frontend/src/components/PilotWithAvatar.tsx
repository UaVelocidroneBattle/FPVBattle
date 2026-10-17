import 'flag-icons/css/flag-icons.min.css';
import { Link } from "react-router-dom";

interface PilotWithAvatarProps {
    name: string;
    countryCode: string | null;
    /** Tag of the pilot's team, shown before the name. */
    teamTag?: string | null;
}

function PilotWithAvatar({ name, countryCode, teamTag }: PilotWithAvatarProps) {
    const hasFlag = countryCode && countryCode.length === 2;

    return (
        <Link
            to={`/pilot/${encodeURIComponent(name)}`}
            className="flex items-center gap-3 group"
            title={`View ${name}'s profile`}
        >
            {hasFlag ? (
                <span
                    className={`fi fi-${countryCode.toLowerCase()} flex-shrink-0 rounded-full ring-slate-600 group-hover:ring-emerald-500 transition-all`}
                    style={{ width: "2.25rem", height: "2.25rem", display: "block", backgroundSize: "cover" }}
                />
            ) : (
                <span className="w-9 h-9 rounded-full flex-shrink-0 bg-slate-700 ring-2 ring-slate-600 flex items-center justify-center text-slate-400 text-xs font-bold">
                    ?
                </span>
            )}
            <span className="flex items-baseline gap-2">
                {teamTag && <span className="text-sm font-semibold tracking-wide text-slate-500">[{teamTag}]</span>}
                <span className="text-sm font-medium text-slate-200 group-hover:text-emerald-400 transition-colors">{name}</span>
            </span>
        </Link>
    );
}

export default PilotWithAvatar;
