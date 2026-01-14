import ComboBox from '@/components/ComboBox';

interface PilotComboBoxProps {
    pilots: string[];
    selectedPilot: string | null;
    selectedPilots: (string | null)[];
    onPilotSelect: (pilot: string) => void;
}

const pilotKey = (pilot: string) => pilot;
const pilotLabel = (pilot: string) => pilot;

const PilotComboBox = ({ pilots, selectedPilot, selectedPilots, onPilotSelect }: PilotComboBoxProps) => {
    const availablePilots = pilots.filter(p => !selectedPilots.some(sp => sp === p) || selectedPilot == p);
    return (
        <div className="flex items-center gap-2">
            <ComboBox
                defaultCaption='Select a pilot'
                items={availablePilots}
                getKey={pilotKey}
                getLabel={pilotLabel}
                onSelect={onPilotSelect}
                value={selectedPilot}
            />
        </div>
    );
};

export default PilotComboBox;