import { PilotProfileModel } from '@/api/client';
import StatCard from '@/components/ui/StatCard';
import { formatDate } from '@/lib/utils';

interface PilotStatsGridProps {
    profile: PilotProfileModel;
}

export function PilotStatsGrid({ profile }: PilotStatsGridProps) {
    return (
        <div className="grid grid-cols-7 gap-4">
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