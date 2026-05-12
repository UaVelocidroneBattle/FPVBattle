import { LucideIcon } from 'lucide-react';

export interface StatCardProps {
    value: string | number;
    label: string;
    icon?: LucideIcon;
    valueColor?: string;
    size?: 'sm' | 'md' | 'lg';
}

const StatCard = ({
                      value,
                      label,
                      icon: Icon,
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
        <div className={`relative bg-slate-700 border-t-2 border-emerald-400 flex flex-col justify-center h-full overflow-hidden ${classes.container}`}>
            {Icon && (
                <Icon size={80} className="absolute -right-4 top-1/2 -translate-y-1/2 text-slate-600 opacity-50 rotate-12" />
            )}
            <div className={`relative ${classes.value} ${valueColor}`}>{value}</div>
            <div className={`relative ${classes.label} text-gray-400`}>{label}</div>
        </div>
    );
};

export default StatCard;
