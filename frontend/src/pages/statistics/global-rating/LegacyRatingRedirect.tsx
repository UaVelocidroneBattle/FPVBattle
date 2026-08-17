import { Navigate, useParams, useSearchParams } from "react-router-dom";

/**
 * The global rating used to live under /statistics. Keeps shared and indexed
 * links working by forwarding them, cup and country filter included.
 */
function LegacyRatingRedirect() {
    const { cupId } = useParams();
    const [searchParams] = useSearchParams();

    const query = searchParams.toString();

    return <Navigate to={`/global-rating${cupId ? `/${cupId}` : ""}${query ? `?${query}` : ""}`} replace />;
}

export default LegacyRatingRedirect;
