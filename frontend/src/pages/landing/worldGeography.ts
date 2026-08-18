import { feature } from 'topojson-client';
import { geoPath } from 'd3-geo';
import type { Topology, GeometryCollection } from 'topojson-specification';
import geoData from 'world-atlas/countries-110m.json';

const UKRAINE_ID = '804';
const RUSSIA_ID = '643';

// Crimea's bounding box (lon/lat) in this dataset — used to isolate it as a disconnected
// piece of Russia's MultiPolygon geometry below.
const CRIMEA_BOUNDS = { minLon: 30, maxLon: 38, minLat: 43, maxLat: 47 };

/**
 * The Natural Earth data behind world-atlas attributes Crimea to Russia. That doesn't
 * reflect the internationally recognized border (Crimea is Ukrainian territory under
 * UN General Assembly resolution 68/262), so we reassign Crimea's polygon from Russia's
 * geometry to Ukraine's before rendering.
 */
function reassignCrimeaToUkraine(features: GeoJSON.Feature[]): GeoJSON.Feature[] {
    const russia = features.find(f => f.id === RUSSIA_ID);
    const ukraine = features.find(f => f.id === UKRAINE_ID);
    if (!russia || !ukraine || russia.geometry.type !== 'MultiPolygon') {
        return features;
    }

    const path = geoPath();
    const crimeaPolygons: GeoJSON.Position[][][] = [];
    const remainingPolygons: GeoJSON.Position[][][] = [];

    for (const polygon of russia.geometry.coordinates) {
        const bounds = path.bounds({ type: 'Feature', properties: {}, geometry: { type: 'Polygon', coordinates: polygon } });
        const isCrimea =
            bounds[0][0] >= CRIMEA_BOUNDS.minLon &&
            bounds[1][0] <= CRIMEA_BOUNDS.maxLon &&
            bounds[0][1] >= CRIMEA_BOUNDS.minLat &&
            bounds[1][1] <= CRIMEA_BOUNDS.maxLat;
        (isCrimea ? crimeaPolygons : remainingPolygons).push(polygon);
    }

    if (crimeaPolygons.length === 0) {
        return features;
    }

    const ukraineGeometry = ukraine.geometry;
    const ukrainePolygons: GeoJSON.Position[][][] =
        ukraineGeometry.type === 'MultiPolygon'
            ? ukraineGeometry.coordinates
            : ukraineGeometry.type === 'Polygon'
                ? [ukraineGeometry.coordinates]
                : [];

    return features.map(f => {
        if (f.id === RUSSIA_ID) {
            return { ...f, geometry: { type: 'MultiPolygon', coordinates: remainingPolygons } };
        }
        if (f.id === UKRAINE_ID) {
            return { ...f, geometry: { type: 'MultiPolygon', coordinates: [...ukrainePolygons, ...crimeaPolygons] } };
        }
        return f;
    });
}

const worldTopology = geoData as unknown as Topology<{ countries: GeometryCollection }>;
const rawFeatures = feature(worldTopology, worldTopology.objects.countries).features;

export const worldFeatures: GeoJSON.FeatureCollection = {
    type: 'FeatureCollection',
    features: reassignCrimeaToUkraine(rawFeatures),
};
