import { PilotAchievementModel } from '@/api/client';
import { formatDate } from '@/lib/utils';
import { useState } from 'react';

export interface AchievementCardProps {
    achievement: PilotAchievementModel;
}

const AchievementCard = ({ achievement }: AchievementCardProps) => {
    const [isFlipped, setIsFlipped] = useState(false);

    return (
        <div 
            className="relative w-full h-20 cursor-pointer"
            onMouseEnter={() => setIsFlipped(true)}
            onMouseLeave={() => setIsFlipped(false)}
            style={{ perspective: '1000px' }}
        >
            <div 
                className="relative w-full h-full transition-transform duration-500"
                style={{
                    transformStyle: 'preserve-3d',
                    transform: isFlipped ? 'rotateY(180deg)' : 'rotateY(0deg)'
                }}
            >
                {/* Front of card */}
                <div 
                    className="absolute inset-0 w-full h-full bg-slate-700 rounded-lg p-4 flex flex-col justify-between hover:bg-slate-600 transition-colors duration-200"
                    style={{ backfaceVisibility: 'hidden' }}
                >
                    <div className="text-sm font-medium text-white">
                        {achievement.title}
                    </div>
                    <div className="text-xs text-gray-400">
                        {formatDate(achievement.earnedOn)}
                    </div>
                </div>

                {/* Back of card */}
                <div 
                    className="absolute inset-0 w-full h-full bg-slate-600 rounded-lg p-4 flex items-center justify-center"
                    style={{ 
                        backfaceVisibility: 'hidden',
                        transform: 'rotateY(180deg)'
                    }}
                >
                    <div className="text-xs text-white text-center leading-relaxed">
                        {achievement.description}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default AchievementCard;