import { Link } from 'react-router-dom';
import { ArrowRight } from 'lucide-react';
import HeroStats from './HeroStats';
import { LandingDataModel } from '@/api/client';

interface HeroSectionProps {
    data: LandingDataModel;
}

function HeroSection({ data }: HeroSectionProps) {
    return (
        <div className="grid lg:grid-cols-2 gap-8 items-start">
            <div className="flex flex-col gap-6">
                <div className="flex flex-col gap-3">
                    <h1 className="text-4xl sm:text-5xl font-bold text-slate-200 leading-tight">
                        The <span className="text-emerald-400">fastest</span> sim pilots are here.
                    </h1>
                    <p className="text-lg text-slate-300 leading-relaxed">
                        FPV Battle turns Velocidrone into a daily competition. A new track every day. Points, rankings, day streaks, achievements and everything you need for real fun.
                    </p>
                </div>
                <Link
                    to="/guide"
                    className="inline-flex items-center gap-2 self-start bg-emerald-500 hover:bg-emerald-400 text-slate-900 font-medium px-6 py-3 transition-colors"
                >
                    Full instructions
                    <ArrowRight className="h-4 w-4" />
                </Link>
            </div>
            <HeroStats data={data} />
        </div>
    );
}

export default HeroSection;
