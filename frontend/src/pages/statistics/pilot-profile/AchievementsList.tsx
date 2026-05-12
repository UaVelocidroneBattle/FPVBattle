import { PilotAchievementModel } from '@/api/client';
import AchievementCard from '@/components/ui/AchievementCard';

export interface AchievementsListProps {
    achievements: PilotAchievementModel[];
}

const AchievementsList = ({ achievements }: AchievementsListProps) => {
    return (
        <>
            {achievements.length === 0 ? (
                <div className="text-gray-500 text-sm text-center py-8">
                    No achievements yet
                </div>
            ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4">
                    {[...achievements].sort((a, b) => (a.achievedOn == null ? 1 : 0) - (b.achievedOn == null ? 1 : 0)).map((achievement, index) => (
                        <AchievementCard
                            key={index}
                            achievement={achievement}
                        />
                    ))}
                </div>
            )}
        </>
    );
};

export default AchievementsList;