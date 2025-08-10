interface PilotColorChipProps {
    color: string;
}

const PilotColorChip = ({ color }: PilotColorChipProps) => {
    const isTransparent = color === 'transparent';
    return (
        <div 
            className={`w-3 h-3 rounded-full ${!isTransparent ? 'border border-gray-400/30' : ''}`}
            style={{ backgroundColor: color }}
        />
    );
};

export default PilotColorChip;