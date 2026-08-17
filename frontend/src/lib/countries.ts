const regionNames = new Intl.DisplayNames(['en'], { type: 'region' });

/**
 * English name for an ISO 3166-1 alpha-2 country code, e.g. 'UA' → 'Ukraine'.
 * Codes the browser does not recognise are returned unchanged.
 */
export function countryName(code: string): string {
    try {
        return regionNames.of(code.toUpperCase()) ?? code;
    } catch {
        return code;
    }
}
