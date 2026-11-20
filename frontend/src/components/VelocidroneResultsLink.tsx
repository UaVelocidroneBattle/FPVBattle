import { ExternalLink } from "lucide-react";
import { Link } from "react-router-dom";

interface IVelocidroneResultLink {
    SceneId: number,
    TrackId: number
}

interface IVelocidroneResultLinkProps {
    trackInfo: IVelocidroneResultLink
}

function VelocdroneResultLink({ trackInfo }: IVelocidroneResultLinkProps) {
    if (!trackInfo) return <></>;
    return <>
        <Link
            className="text-sm text-slate-400 hover:text-emerald-400 transition-colors inline-flex items-center gap-2"
            to={`https://www.velocidrone.com/leaderboard/${trackInfo.SceneId}/${trackInfo.TrackId}/All`}
            target="_blank">
            Velocidrone leaderboard
            <ExternalLink className="w-4 h-4"></ExternalLink>
        </Link>
    </>
}

export default VelocdroneResultLink;