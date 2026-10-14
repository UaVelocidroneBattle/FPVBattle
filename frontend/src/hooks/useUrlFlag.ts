import { useSearchParams } from "react-router-dom";

/**
 * A yes/no switch kept in the URL (?flat=1), so the page can be bookmarked or
 * shared the way it is set. The URL is the single source of truth; a missing
 * param reads as off. Switching replaces the history entry: it is state, not
 * navigation.
 */
export function useUrlFlag(param: string): [boolean, (value: boolean) => void] {
    const [searchParams, setSearchParams] = useSearchParams();

    const value = searchParams.get(param) === "1";

    const set = (enabled: boolean) => {
        setSearchParams((previous) => {
            const next = new URLSearchParams(previous);

            if (enabled) {
                next.set(param, "1");
            } else {
                next.delete(param);
            }

            return next;
        }, { replace: true });
    };

    return [value, set];
}
