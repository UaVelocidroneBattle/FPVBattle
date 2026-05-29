import { NavLink, Outlet } from 'react-router-dom';
import { useLanguage, type Language } from '@/hooks/useLanguage';
import { navItems } from './translations';

function LanguageToggle({ language, setLanguage }: { language: Language; setLanguage: (lang: Language) => void }) {
    return (
        <div className="flex gap-1 border border-slate-600 p-0.5 text-sm font-medium">
            {(['ua', 'en'] as Language[]).map((lang) => (
                <button
                    key={lang}
                    onClick={() => setLanguage(lang)}
                    className={`px-3 py-1 transition-colors uppercase ${
                        language === lang ? 'bg-slate-600 text-slate-200' : 'text-slate-500 hover:text-slate-300'
                    }`}
                >
                    {lang}
                </button>
            ))}
        </div>
    );
}

function RulesLayout() {
    const { language, setLanguage } = useLanguage();

    return (
        <div className="flex flex-col md:flex-row md:items-start gap-4 md:gap-6 flex-1">
            <aside className="md:w-64 md:shrink-0 md:sticky md:top-4 md:self-start">
                <div className="bg-slate-800/50 backdrop-blur-sm border border-slate-700 overflow-hidden">
                    <div className="px-4 py-3 border-b border-slate-700 flex justify-end">
                        <LanguageToggle language={language} setLanguage={setLanguage} />
                    </div>
                    <nav className="flex md:flex-col overflow-x-auto py-2 md:py-2">
                        {navItems.map(item => (
                            <NavLink
                                key={item.path}
                                to={item.path}
                                className={({ isActive }) =>
                                    `shrink-0 block px-5 py-3 text-base transition-colors md:border-l-2 border-b-2 md:border-b-0 ${
                                        isActive
                                            ? 'border-emerald-400 text-emerald-400 bg-emerald-400/5'
                                            : 'border-transparent text-slate-400 hover:text-slate-200 hover:bg-slate-700/30'
                                    }`
                                }
                            >
                                {item.label[language]}
                            </NavLink>
                        ))}
                    </nav>
                </div>
            </aside>

            <div className="flex-1 min-w-0">
                <div className="bg-slate-800/50 backdrop-blur-sm border border-slate-700 p-6">
                    <div className="max-w-prose">
                        <Outlet />
                    </div>
                </div>
            </div>
        </div>
    );
}

export default RulesLayout;
