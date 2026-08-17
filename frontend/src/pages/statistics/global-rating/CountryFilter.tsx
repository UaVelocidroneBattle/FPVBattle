import ComboBox from "@/components/ComboBox";
import CountryFlag from "@/components/ui/CountryFlag";
import { countryName } from "@/lib/countries";

export interface CountryOption {
    /** ISO 3166-1 alpha-2 code, or null for "All". */
    code: string | null;
    name: string;
}

export const ALL_COUNTRIES: CountryOption = { code: null, name: "All" };

/**
 * The countries represented in the given pilots, named and sorted, with "All"
 * first. Pilots without a country contribute nothing.
 */
export function countryOptionsOf(pilots: { country: string | null }[]): CountryOption[] {
    const codes = [...new Set(pilots.map(pilot => pilot.country).filter((code): code is string => !!code))];

    const countries = codes
        .map(code => ({ code, name: countryName(code) }))
        .sort((a, b) => a.name.localeCompare(b.name));

    return [ALL_COUNTRIES, ...countries];
}

function CountryLabel({ option }: { option: CountryOption }) {
    return (
        <span className="flex items-center gap-2 truncate">
            {option.code && <CountryFlag countryCode={option.code} className="rounded-sm" />}
            {option.name}
        </span>
    );
}

interface CountryFilterProps {
    options: CountryOption[];
    value: CountryOption;
    onChange: (option: CountryOption) => void;
}

function CountryFilter({ options, value, onChange }: CountryFilterProps) {
    return (
        <div className="flex items-center gap-2">
            <span className="text-sm text-slate-400">By country</span>
            <ComboBox
                defaultCaption={ALL_COUNTRIES.name}
                items={options}
                getKey={option => option.code ?? "all"}
                getLabel={option => option.name}
                renderLabel={option => <CountryLabel option={option} />}
                value={value}
                onSelect={onChange}
            />
        </div>
    );
}

export default CountryFilter;
