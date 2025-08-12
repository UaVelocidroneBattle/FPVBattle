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
                label="Current Streak"
                valueColor="text-green-400"
            />

            <StatCard
                value={profile.maxDayStreak}
                label="Max Streak"
                valueColor="text-yellow-400"
            />

            <StatCard
                value={profile.totalRaceDays}
                label="Total Race Days"
                valueColor="text-blue-400"
            />

            <StatCard
                value={profile.achievements.length}
                label="Achievements"
                valueColor="text-purple-400"
            />

            <StatCard
                value={profile.availableFreezes}
                label="Freezes"
                valueColor="text-cyan-400"
            />

            <StatCard
                value={formatDate(profile.lastRaceDate)}
                label="Last Race"
                valueColor="text-gray-300"
            />

            <StatCard
                value={formatDate(profile.firstRaceDate)}
                label="First Race"
                valueColor="text-orange-400"
            />
        </div>
    );
}

export default PilotStatsGrid;