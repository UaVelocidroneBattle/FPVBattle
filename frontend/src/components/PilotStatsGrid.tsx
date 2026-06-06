import { PilotProfileModel } from '@/api/client';
import StatCard from '@/components/ui/StatCard';
import { Calendar, Flame, Shield, Snowflake, TrendingUp, Trophy } from 'lucide-react';

interface PilotStatsGridProps {
    profile: PilotProfileModel;
}

export function PilotStatsGrid({ profile }: PilotStatsGridProps) {
    return (
        <div className="grid gap-4 grid-cols-[repeat(auto-fit,minmax(150px,1fr))]">
            {profile.league != null && (
                <div
                    className="relative bg-slate-700 flex flex-col justify-center h-full overflow-hidden p-4"
                    style={{ borderTop: `2px solid ${profile.leagueColor || '#34d399'}` }}
                >
                    <Shield size={80} className="absolute -right-4 top-1/2 -translate-y-1/2 text-slate-600 opacity-50 rotate-12" />
                    <div
                        className="relative text-2xl font-bold"
                        style={{ color: profile.leagueColor || '#34d399' }}
                    >
                        {profile.league}
                    </div>
                    <div className="relative text-sm text-gray-400">League</div>
                </div>
            )}

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
