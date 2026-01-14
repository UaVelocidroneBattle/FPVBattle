import { PilotProfileModel } from '@/api/client';
import StatCard from '@/components/ui/StatCard';

interface PilotStatsGridProps {
    profile: PilotProfileModel;
}

export function PilotStatsGrid({ profile }: PilotStatsGridProps) {
    return (
        <div className="grid gap-4 grid-cols-[repeat(auto-fit,minmax(150px,1fr))]">
            <StatCard
                value={profile.currentDayStreak}
                label="Current streak"
                valueColor="text-emerald-400"
            />

            <StatCard
                value={profile.maxDayStreak}
                label="Max streak"
                valueColor="text-emerald-400"
            />

            <StatCard
                value={profile.totalRaceDays}
                label="Total race days"
                valueColor="text-emerald-400"
            />

            <StatCard
                value={profile.achievements.length}
                label="Achievements"
                valueColor="text-emerald-400"
            />

            <StatCard
                value={profile.availableFreezes}
                label="Freezes"
                valueColor="text-emerald-400"
            />
        </div>
    );
}

export default PilotStatsGrid;