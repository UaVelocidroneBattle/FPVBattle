import { useSearchParams } from "react-router-dom";
import { ALL_COUNTRIES, CountryOption } from "./CountryFilter";

const COUNTRY_PARAM = "country";

/**
 * Keeps the selected country in the URL, so a country's leaderboard can be
 * bookmarked or shared: /global-rating/open-class?country=ua
 *
 * The URL is the single source of truth. A missing code — or one no pilot in
 * this cup flies under — reads as "All", so a hand-edited link still works.
 * Selecting replaces the history entry: the filter is state, not navigation.
 */
export function useUrlCountry(options: CountryOption[]): [CountryOption, (option: CountryOption) => void] {
    const [searchParams, setSearchParams] = useSearchParams();

    const code = searchParams.get(COUNTRY_PARAM);
    const selected = options.find(option => !!option.code && option.code.toLowerCase() === code?.toLowerCase()) ?? ALL_COUNTRIES;

    const select = (option: CountryOption) => {
        setSearchParams((previous) => {
            const next = new URLSearchParams(previous);

            if (option.code) {
                next.set(COUNTRY_PARAM, option.code.toLowerCase());
            } else {
                next.delete(COUNTRY_PARAM);
            }

            return next;
        }, { replace: true });
    };

    return [selected, select];
}
