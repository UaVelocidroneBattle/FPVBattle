import ComboBox from '@/components/ComboBox';

interface PilotSelectorsProps {
    selectedPilots: (string | null)[];
    pilots: string[];
    onPilotChanged: (index: number) => (pilot: string) => void;
}

const pilotKey = (pilot: string) => pilot;
const pilotLabel = (pilot: string) => pilot;

const PilotSelectors = ({ selectedPilots, pilots, onPilotChanged }: PilotSelectorsProps) => {
    return (
        <>
            {selectedPilots.map((sp, index) => (
                <div key={index} className='flex-row'>
                    <ComboBox 
                        defaultCaption='Select a pilot'
                        items={pilots}
                        getKey={pilotKey}
                        getLabel={pilotLabel}
                        onSelect={onPilotChanged(index)}
                        value={sp}
                    />
                </div>
            ))}
        </>
    );
};

export default PilotSelectors;