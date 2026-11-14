import { Link } from "react-router-dom";

interface TeamNameProps {
    teamId?: number;
    name: string;
    tag?: string | null;
    /** A deleted team keeps its place in past results, but has no page to link to. */
    isDeleted?: boolean;
    className?: string;
}

function TeamName({ teamId, name, tag, isDeleted = false, className = "" }: TeamNameProps) {
    return (
        <span className={`flex min-w-0 items-baseline gap-2 ${className}`}>
            {tag && <span className="shrink-0 text-xs font-semibold tracking-wide text-slate-500">[{tag}]</span>}
            {isDeleted || teamId === undefined ? (
                <span className="truncate text-slate-500" title="This team was deleted">{name}</span>
            ) : (
                <Link
                    to={`/teams/${teamId}`}
                    onClick={(e) => e.stopPropagation()}
                    className="truncate hover:text-emerald-400 transition-colors"
                    title={`View ${name}`}
                >
                    {name}
                </Link>
            )}
        </span>
    );
}

export default TeamName;
