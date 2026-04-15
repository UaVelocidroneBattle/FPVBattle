import { PilotAchievementModel } from '@/api/client';
import { formatDate } from '@/lib/utils';
import { CheckCircle } from 'lucide-react';
import { useState } from 'react';

export interface AchievementCardProps {
    achievement: PilotAchievementModel;
}

const AchievementCard = ({ achievement }: AchievementCardProps) => {
    const [isFlipped, setIsFlipped] = useState(false);
    const achieved = achievement.achievedOn != null;

    return (
        <div
            className={`relative w-full h-24 ${achieved ? 'cursor-pointer' : 'cursor-default'}`}
            onMouseEnter={() => achieved && setIsFlipped(true)}
            onMouseLeave={() => achieved && setIsFlipped(false)}
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
                    className={`absolute inset-0 w-full h-full p-4 flex flex-col justify-between transition-colors duration-200 bg-slate-700 ${
                        achieved ? 'hover:bg-slate-600 border-l-4 border-emerald-400' : ''
                    }`}
                    style={{
                        backfaceVisibility: 'hidden',
                        ...(achieved && {
                            backgroundImage: `
                                linear-gradient(rgba(255,255,255,0.04) 1px, transparent 1px),
                                linear-gradient(90deg, rgba(255,255,255,0.04) 1px, transparent 1px)
                            `,
                            backgroundSize: '16px 16px'
                        })
                    }}
                >
                    <div className="text-sm font-medium text-white flex items-center gap-1.5">
                        {achieved && <CheckCircle size={14} className="text-emerald-400 shrink-0" />}
                        {achievement.title}
                    </div>
                    <div className="text-xs text-gray-400">
                        {achievement.description}
                    </div>
                </div>

                {/* Back of card */}
                <div
                    className="absolute inset-0 w-full h-full bg-slate-600 p-4 flex items-center justify-center"
                    style={{
                        backfaceVisibility: 'hidden',
                        transform: 'rotateY(180deg)'
                    }}
                >
                    <div className="text-xs text-white text-center leading-relaxed">
                        <div className='text-gray-400'>Achieved:</div>
                        {formatDate(achievement.achievedOn!)}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default AchievementCard;