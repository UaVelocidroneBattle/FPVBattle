import { Navigate, useParams, useSearchParams } from "react-router-dom";

interface LegacyRedirectProps {
    /** New base path, e.g. "/pilot". */
    to: string;
    /** Route param carried over, e.g. "pilot" or "cupId". */
    param: string;
}

/**
 * Forwards a path a page used to live at, keeping its trailing route param and
 * query string, so links already shared or indexed still land in the right place.
 */
function LegacyRedirect({ to, param }: LegacyRedirectProps) {
    const value = useParams()[param];
    const [searchParams] = useSearchParams();

    const query = searchParams.toString();
    const target = `${to}${value ? `/${encodeURIComponent(value)}` : ""}${query ? `?${query}` : ""}`;

    return <Navigate to={target} replace />;
}

export default LegacyRedirect;
