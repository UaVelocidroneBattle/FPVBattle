import { Users, Globe, Trophy } from 'lucide-react';
import StatCard from '@/components/ui/StatCard';
import { LandingDataModel } from '@/api/client';

interface HeroStatsProps {
    data: LandingDataModel;
}

function HeroStats({ data }: HeroStatsProps) {
    return (
        <div className="bg-slate-800/50 backdrop-blur-sm border border-slate-700 p-5">
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-5">
                <StatCard value={data.totalPilots ?? 0} label="Pilots registered" icon={Users} valueColor="text-emerald-400" size="lg" />
                <StatCard value={data.totalCountries ?? 0} label="Countries" icon={Globe} valueColor="text-emerald-400" size="lg" />
                <StatCard value={data.dailyActivePilots ?? 0} label="Daily active pilots" icon={Trophy} valueColor="text-emerald-400" size="lg" />
            </div>
        </div>
    );
}

export default HeroStats;
