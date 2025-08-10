import ComboBox from '@/components/ComboBox';
import PilotColorChip from '@/components/ui/PilotColorChip';

interface PilotComboBoxProps {
    pilots: string[];
    selectedPilot: string | null;
    selectedPilots: (string | null)[];
    onPilotSelect: (pilot: string) => void;
    color: string;
}

const pilotKey = (pilot: string) => pilot;
const pilotLabel = (pilot: string) => pilot;

const PilotComboBox = ({ pilots, selectedPilot, selectedPilots, onPilotSelect, color }: PilotComboBoxProps) => {
    const availablePilots = pilots.filter(p => !selectedPilots.some(sp => sp === p) || selectedPilot == p);
    
    const getSelectionIcon = (_pilot: string, isSelected: boolean) => (
        <PilotColorChip color={isSelected ? color : 'transparent'} />
    );

    return (
        <ComboBox
            defaultCaption='Select a pilot'
            items={availablePilots}
            getKey={pilotKey}
            getLabel={pilotLabel}
            onSelect={onPilotSelect}
            value={selectedPilot}
            leadingIcon={<PilotColorChip color={color} />}
            getSelectionIcon={getSelectionIcon}
        />
    );
};

export default PilotComboBox;