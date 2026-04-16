import { Link } from "react-router-dom";

interface PilotNameProps {
    name: string;
    className?: string;
}

function PilotName({ name, className = "" }: PilotNameProps) {
    return (
        <Link 
            to={`/statistics/profile/${encodeURIComponent(name)}`}
            className={`hover:text-emerald-400 transition-colors cursor-pointer ${className}`}
            title={`View ${name}'s profile`}
        >
            {name}
        </Link>
    );
}

export default PilotName;