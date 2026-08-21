import { PilotAchievementModel } from '@/api/client';
import AchievementCard from '@/components/ui/AchievementCard';
import { ChevronDown, ChevronUp } from 'lucide-react';
import { useMemo, useState } from 'react';

/** Two full rows on a wide screen: enough to show what a pilot has earned without burying the rest of the profile. */
const PAGE_SIZE = 10;

export interface AchievementsListProps {
    achievements: PilotAchievementModel[];
}

/** Unlocked first, so the collapsed list leads with what the pilot actually has. */
function unlockedFirst(achievements: PilotAchievementModel[]): PilotAchievementModel[] {
    return [...achievements].sort((a, b) => (a.achievedOn == null ? 1 : 0) - (b.achievedOn == null ? 1 : 0));
}

function AchievementGrid({ achievements, className = '' }: { achievements: PilotAchievementModel[]; className?: string }) {
    return (
        <div className={`grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4 ${className}`}>
            {achievements.map((achievement) => (
                <AchievementCard
                    key={achievement.name}
                    achievement={achievement}
                />
            ))}
        </div>
    );
}

const AchievementsList = ({ achievements }: AchievementsListProps) => {
    const [showAll, setShowAll] = useState(false);
    const sorted = useMemo(() => unlockedFirst(achievements), [achievements]);

    if (sorted.length === 0) {
        return (
            <div className="text-gray-500 text-sm text-center py-8">
                No achievements yet
            </div>
        );
    }

    const isExpandable = sorted.length > PAGE_SIZE;

    return (
        <>
            <AchievementGrid achievements={sorted.slice(0, PAGE_SIZE)} />

            {isExpandable && (
                <>
                    {/*
                      * The rest stay mounted and are revealed by growing their row from
                      * 0fr to 1fr — a height that animates without anyone having to
                      * measure the cards, which a plain `height: auto` cannot do.
                      */}
                    <div
                        className={`grid transition-[grid-template-rows] duration-300 ease-out motion-reduce:transition-none ${showAll ? 'grid-rows-[1fr]' : 'grid-rows-[0fr]'}`}
                        aria-hidden={!showAll}
                    >
                        <div className="overflow-hidden">
                            <AchievementGrid achievements={sorted.slice(PAGE_SIZE)} className="pt-4" />
                        </div>
                    </div>

                    <div className="flex justify-center pt-4">
                        <button
                            className="flex items-center text-emerald-400 hover:text-emerald-300 transition-colors text-sm font-medium"
                            onClick={() => setShowAll(!showAll)}
                            aria-expanded={showAll}
                        >
                            {showAll ? 'Show less' : 'Show all'}
                            {showAll
                                ? <ChevronUp className="ml-2 h-4 w-4" />
                                : <ChevronDown className="ml-2 h-4 w-4" />}
                        </button>
                    </div>
                </>
            )}
        </>
    );
};

export default AchievementsList;
