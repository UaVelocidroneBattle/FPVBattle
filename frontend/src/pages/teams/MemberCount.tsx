import { Users } from "lucide-react";

interface MemberCountProps {
    count: number;
    /** The configured team size limit; 0 when there is none. */
    maxTeamSize: number;
}

function MemberCount({ count, maxTeamSize }: MemberCountProps) {
    return (
        <span className="inline-flex items-baseline gap-1.5 whitespace-nowrap" title="Members">
            <Users className="h-3.5 w-3.5 translate-y-0.5 self-center" aria-hidden />
            {maxTeamSize > 0 ? `${count} / ${maxTeamSize}` : count}
        </span>
    );
}

export default MemberCount;
