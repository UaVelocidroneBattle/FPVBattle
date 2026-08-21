import { PilotClassRatingModel, PilotProfileModel } from '@/api/client';
import { Calendar, Flame, LucideIcon, Snowflake, TrendingUp } from 'lucide-react';

export interface RatingSectionProps {
    profile: PilotProfileModel;
}

function ClassRatingCard({ classRating }: { classRating: PilotClassRatingModel }) {
    return (
        <div className="border-l-2 border-emerald-400 pl-4 py-2 flex items-center justify-between gap-4">
            <div>
                <div className="text-2xl font-bold text-emerald-400">
                    {classRating.globalRating != null ? `#${classRating.globalRating}` : '—'}
                </div>
                <div className="text-sm text-gray-400 mt-1">{classRating.className} global rating</div>
            </div>
            {classRating.league && (
                <div className="text-right">
                    <div className="text-2xl font-bold" style={{ color: classRating.leagueColor || '#34d399' }}>
                        {classRating.league}
                    </div>
                    <div className="text-sm text-gray-400 mt-1">League</div>
                </div>
            )}
        </div>
    );
}

interface StatRowProps {
    icon: LucideIcon;
    label: string;
    value: string | number;
}

function StatRow({ icon: Icon, label, value }: StatRowProps) {
    return (
        <div className="flex items-center justify-between py-2.5 border-b border-slate-700/50 last:border-b-0">
            <span className="flex items-center gap-2.5 text-sm text-gray-400">
                <Icon size={15} className="text-slate-500" />
                {label}
            </span>
            <span className="text-base font-semibold text-white tabular-nums">{value}</span>
        </div>
    );
}

function RatingSection({ profile }: RatingSectionProps) {
    if (profile.classRatings.length === 0) return null;

    return (
        <div className="grid gap-6 md:grid-cols-2">
            <div className="bg-slate-800 p-6">
                <h2 className="text-xl font-semibold text-white mb-4">Rating</h2>
                <div className="flex flex-col gap-5">
                    {profile.classRatings.map(classRating => (
                        <ClassRatingCard key={classRating.cupId} classRating={classRating} />
                    ))}
                </div>
            </div>

            <div className="bg-slate-800 p-6">
                <h2 className="text-xl font-semibold text-white mb-4">Stats</h2>
                <div className="flex flex-col">
                    <StatRow icon={Flame} label="Current streak" value={profile.currentDayStreak} />
                    <StatRow icon={TrendingUp} label="Max streak" value={profile.maxDayStreak} />
                    <StatRow icon={Snowflake} label="Freezes" value={profile.availableFreezes} />
                    <StatRow icon={Calendar} label="Total race days" value={profile.totalRaceDays} />
                </div>
            </div>
        </div>
    );
}

export default RatingSection;
