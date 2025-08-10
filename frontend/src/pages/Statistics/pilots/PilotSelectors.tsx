import PilotComboBox from './PilotComboBox';
import { PILOT_COLORS } from './chartColors';

interface PilotSelectorsProps {
    selectedPilots: (string | null)[];
    pilots: string[];
    onPilotChanged: (index: number) => (pilot: string) => void;
}

const PilotSelectors = ({ selectedPilots, pilots, onPilotChanged }: PilotSelectorsProps) => {
    return (
        <>
            {selectedPilots.map((sp, index) => (
                <div key={index} className='flex-row'>
                    <PilotComboBox 
                        pilots={pilots}
                        selectedPilot={sp}
                        selectedPilots={selectedPilots}
                        onPilotSelect={onPilotChanged(index)}
                        color={PILOT_COLORS[index] || PILOT_COLORS[0]}
                    />
                </div>
            ))}
        </>
    );
};

export default PilotSelectors;