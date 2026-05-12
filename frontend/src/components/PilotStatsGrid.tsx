import { PilotProfileModel } from '@/api/client';
import StatCard from '@/components/ui/StatCard';
import { Calendar, Flame, Snowflake, TrendingUp, Trophy } from 'lucide-react';

interface PilotStatsGridProps {
    profile: PilotProfileModel;
}

export function PilotStatsGrid({ profile }: PilotStatsGridProps) {
    return (
        <div className="grid gap-4 grid-cols-[repeat(auto-fit,minmax(150px,1fr))]">
            <StatCard
                value={profile.globalRating != null ? `#${profile.globalRating}` : '—'}
                label="Global rating"
                icon={Trophy}
                valueColor="text-emerald-400"
            />

            <StatCard
                value={profile.currentDayStreak}
                label="Current streak"
                icon={Flame}
                valueColor="text-emerald-400"
            />

            <StatCard
                value={profile.maxDayStreak}
                label="Max streak"
                icon={TrendingUp}
                valueColor="text-emerald-400"
            />

            <StatCard
                value={profile.totalRaceDays}
                label="Total race days"
                icon={Calendar}
                valueColor="text-emerald-400"
            />

<StatCard
                value={profile.availableFreezes}
                label="Freezes"
                icon={Snowflake}
                valueColor="text-emerald-400"
            />
        </div>
    );
}

export default PilotStatsGrid;
