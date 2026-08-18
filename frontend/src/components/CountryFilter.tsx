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
        <span className="flex items-center gap-1.5 truncate">
            {option.code && <CountryFlag countryCode={option.code} className="rounded-sm" />}
            {option.name}
        </span>
    );
}

interface CountryFilterProps {
    options: CountryOption[];
    value: CountryOption;
    onChange: (option: CountryOption) => void;
    /** Follows the type around it: "sm" among a leaderboard's controls, "md" beside a page title. */
    size?: "sm" | "md";
}

const textSizes = { sm: "text-xs", md: "text-sm" };

/** A muted label, then the selection as plain text — no input box to draw the eye. */
function CountryFilter({ options, value, onChange, size = "sm" }: CountryFilterProps) {
    return (
        <div className="flex items-center gap-3">
            <span className={`${textSizes[size]} text-slate-400`}>By country</span>
            <ComboBox
                variant="inline"
                triggerClassName={textSizes[size]}
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
