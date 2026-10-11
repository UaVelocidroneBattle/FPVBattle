import { Link } from 'react-router-dom';
import { Users } from 'lucide-react';
import type { PilotTeamModel } from '@/api/client';
import { useTeamsStore } from '@/store/teamsStore';

interface PilotTeamsProps {
    teams: PilotTeamModel[];
}

function PilotTeams({ teams }: PilotTeamsProps) {
    if (teams.length === 0) return null;

    return (
        <div className="flex flex-wrap items-center gap-2 min-h-7 sm:min-h-9 sm:justify-end">
            {teams.map((team) => (
                <Link
                    key={team.id}
                    to="/teams"
                    onClick={() => useTeamsStore.getState().selectTeam(team.id)}
                    className="flex items-center gap-2 text-xs sm:text-sm font-medium text-slate-400 bg-slate-700/60 border border-slate-600 rounded-full pl-3 pr-4 py-1 transition-colors hover:border-emerald-400 hover:text-emerald-400"
                >
                    <Users className="size-3.5 sm:size-4 text-emerald-400" />
                    {team.name}
                </Link>
            ))}
        </div>
    );
}

export default PilotTeams;
