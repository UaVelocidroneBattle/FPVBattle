interface TeamScopeSwitchProps {
    /** True when the board shows only the user's team. */
    myTeam: boolean;
    onChange: (myTeam: boolean) => void;
}

const OPTIONS = [
    { label: "All", myTeam: false },
    { label: "My team", myTeam: true },
];

/** Switches a leaderboard between everyone and the signed-in user's own team. */
function TeamScopeSwitch({ myTeam, onChange }: TeamScopeSwitchProps) {
    return (
        <div role="group" aria-label="Leaderboard scope" className="flex border border-slate-600 text-xs">
            {OPTIONS.map((option) => (
                <button
                    key={option.label}
                    type="button"
                    aria-pressed={myTeam === option.myTeam}
                    onClick={() => onChange(option.myTeam)}
                    className={`px-2.5 py-1 transition-colors ${
                        myTeam === option.myTeam
                            ? "bg-emerald-500/15 text-emerald-400"
                            : "text-slate-400 hover:text-emerald-400"
                    }`}
                >
                    {option.label}
                </button>
            ))}
        </div>
    );
}

export default TeamScopeSwitch;
