import SegmentedControl from "@/components/ui/SegmentedControl";

interface TeamScopeSwitchProps {
    /** True when the board shows only the user's team. */
    myTeam: boolean;
    onChange: (myTeam: boolean) => void;
}

const OPTIONS = [
    { label: "All", value: false },
    { label: "My team", value: true },
];

/** Switches a leaderboard between everyone and the signed-in user's own team. */
function TeamScopeSwitch({ myTeam, onChange }: TeamScopeSwitchProps) {
    return <SegmentedControl options={OPTIONS} value={myTeam} onChange={onChange} ariaLabel="Leaderboard scope" />;
}

export default TeamScopeSwitch;
