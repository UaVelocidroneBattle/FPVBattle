import PilotComboBox from './PilotComboBox';

interface PilotSelectorsProps {
    selectedPilots: (string | null)[];
    pilots: string[];
    onPilotChanged: (index: number) => (pilot: string) => void;
}

const PilotSelectors = ({ selectedPilots, pilots, onPilotChanged }: PilotSelectorsProps) => {
    return (
        <>
            {selectedPilots.map((sp, index) => (
                <div key={index} className='flex-row mr-3'>
                    <PilotComboBox 
                        pilots={pilots}
                        selectedPilot={sp}
                        selectedPilots={selectedPilots}
                        onPilotSelect={onPilotChanged(index)}
                    />
                </div>
            ))}
        </>
    );
};

export default PilotSelectors;