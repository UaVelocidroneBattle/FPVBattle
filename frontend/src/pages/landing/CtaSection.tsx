import { Link } from 'react-router-dom';
import { ArrowRight } from 'lucide-react';

function CtaSection() {
    return (
        <div className="bg-slate-800/50 backdrop-blur-sm border border-slate-700 border-t-2 border-t-emerald-400 p-8 lg:p-10">
            <div className="grid lg:grid-cols-2 gap-10 items-center">
                <div className="flex flex-col gap-3">
                    <h2 className="text-3xl font-bold text-white">Ready for today's track?</h2>
                    <p className="text-lg text-slate-300">
                        Choose and fly a track from two classes: Open or Whoop. Or fly them both.
                    </p>
                </div>
                <div className="flex flex-col sm:items-end">
                    <div className="flex flex-col gap-3 sm:inline-flex">
                        <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                            <Link
                                to="/open"
                                className="text-center bg-emerald-500 hover:bg-emerald-400 text-slate-900 font-medium px-6 py-3 transition-colors whitespace-nowrap"
                            >
                                Open Class leaderboard
                            </Link>
                            <Link
                                to="/whoop"
                                className="text-center bg-emerald-500 hover:bg-emerald-400 text-slate-900 font-medium px-6 py-3 transition-colors whitespace-nowrap"
                            >
                                Whoop Class leaderboard
                            </Link>
                        </div>
                        <Link
                            to="/guide"
                            className="flex items-center justify-center gap-2 border border-emerald-500 text-emerald-400 hover:text-emerald-300 hover:border-emerald-400 font-medium px-6 py-3 transition-colors"
                        >
                            Read full instructions
                            <ArrowRight className="h-4 w-4" />
                        </Link>
                    </div>
                </div>
            </div>
        </div>
    );
}

export default CtaSection;
