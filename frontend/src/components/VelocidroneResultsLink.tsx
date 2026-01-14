import { ExternalLink } from "lucide-react";
import { Link } from "react-router-dom";

interface IVelocidroneResultLink {
    MapId: number,
    TrackId: number
}

interface IVelocidroneResultLinkProps {
    trackInfo: IVelocidroneResultLink
}

const VelocdroneResultLink: React.FC<IVelocidroneResultLinkProps> = ({ trackInfo }) => {
    if (!trackInfo) return <></>;
    return <>
        <Link
            className="text-sm text-slate-400 hover:text-emerald-400 transition-colors inline-flex items-center gap-2"
            to={`https://www.velocidrone.com/leaderboard/${trackInfo.MapId}/${trackInfo.TrackId}/All`}
            target="_blank">
            Velocidrone leaderboard
            <ExternalLink className="w-4 h-4"></ExternalLink>
        </Link>
    </>
}

export default VelocdroneResultLink;