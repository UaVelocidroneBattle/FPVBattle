import { alpha2ToNumeric } from 'i18n-iso-countries';

/**
 * Converts an ISO 3166-1 alpha-2 country code (e.g. "ua") to the ISO 3166-1
 * numeric code used as the `id` field in the world-atlas TopoJSON (see WorldMap.tsx).
 */
export function getNumericCountryId(alpha2CountryCode: string): string | undefined {
    return alpha2ToNumeric(alpha2CountryCode.toUpperCase());
}
