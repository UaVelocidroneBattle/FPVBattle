import 'flag-icons/css/flag-icons.min.css';

interface CountryFlagProps {
    countryCode: string;
    className?: string;
}

function CountryFlag({ countryCode, className = "" }: CountryFlagProps) {
    if (!countryCode || countryCode.length !== 2) return null;

    return (
        <span
            className={`fi fi-${countryCode.toLowerCase()} ${className}`}
            title={countryCode}
        />
    );
}

export default CountryFlag;
