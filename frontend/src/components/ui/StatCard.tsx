export interface StatCardProps {
    value: string | number;
    label: string;
    valueColor?: string;
    size?: 'sm' | 'md' | 'lg';
}

const StatCard = ({
                      value,
                      label,
                      valueColor = 'text-white',
                      size = 'md'
                  }: StatCardProps) => {
    const sizeClasses = {
        sm: { container: 'p-3', value: 'text-lg font-bold', label: 'text-xs' },
        md: { container: 'p-4', value: 'text-2xl font-bold', label: 'text-sm' },
        lg: { container: 'p-6', value: 'text-3xl font-bold', label: 'text-base' },
    };

    const classes = sizeClasses[size];

    return (
        <div className={`bg-slate-700 rounded text-center flex flex-col justify-center h-full ${classes.container}`}>
            <div className={`${classes.value} ${valueColor}`}>{value}</div>
            <div className={`${classes.label} text-gray-300`}>{label}</div>
        </div>
    );
};

export default StatCard;