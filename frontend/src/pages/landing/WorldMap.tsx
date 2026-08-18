import { useMemo, useRef, useState } from 'react';
import { ComposableMap, Geographies, Geography, type ProjectionFunction } from 'react-simple-maps';
import { geoEqualEarth } from 'd3-geo';
import CountryFlag from '@/components/ui/CountryFlag';
import { CountryPilotsModel } from '@/api/client';
import { getNumericCountryId } from './countryNumericId';
import { worldFeatures } from './worldGeography';

const MAP_WIDTH = 800;
const MAP_HEIGHT = 420;
const MAP_PADDING = 12;

// Fit the projection to the whole world (Antarctica included) so the map fills the frame
// without leaving dead space or clipping any country at the edges.
// react-simple-maps accepts an already-configured GeoProjection instance here at runtime
// (it just passes the function through), even though its types only declare the
// width/height factory shape.
const mapProjection = geoEqualEarth().fitExtent(
    [
        [MAP_PADDING, MAP_PADDING],
        [MAP_WIDTH - MAP_PADDING, MAP_HEIGHT - MAP_PADDING],
    ],
    worldFeatures
) as unknown as ProjectionFunction;

interface TooltipState {
    row: CountryPilotsModel;
    x: number;
    y: number;
}

interface WorldMapProps {
    countryPilots: CountryPilotsModel[];
}

function WorldMap({ countryPilots }: WorldMapProps) {
    const [tooltip, setTooltip] = useState<TooltipState | null>(null);
    const containerRef = useRef<HTMLDivElement>(null);

    const rowByNumericId = useMemo(() => {
        const map = new Map<string, CountryPilotsModel>();
        for (const row of countryPilots) {
            if (!row.countryCode) continue;
            const numericId = getNumericCountryId(row.countryCode);
            if (numericId) map.set(numericId, row);
        }
        return map;
    }, [countryPilots]);

    const trackTooltip = (row: CountryPilotsModel, evt: { clientX: number; clientY: number }) => {
        const rect = containerRef.current?.getBoundingClientRect();
        if (!rect) return;
        setTooltip({ row, x: evt.clientX - rect.left, y: evt.clientY - rect.top });
    };

    return (
        <div ref={containerRef} className="relative">
            <ComposableMap
                projection={mapProjection}
                width={MAP_WIDTH}
                height={MAP_HEIGHT}
                className="relative w-full h-auto"
            >
                <Geographies geography={worldFeatures}>
                    {({ geographies }) =>
                        geographies.map(geo => {
                            const row = rowByNumericId.get(geo.id as string);
                            const isHighlighted = !!row;

                            return (
                                <Geography
                                    key={geo.rsmKey}
                                    geography={geo}
                                    onMouseEnter={evt => {
                                        if (row) trackTooltip(row, evt);
                                    }}
                                    onMouseMove={evt => {
                                        if (row) trackTooltip(row, evt);
                                    }}
                                    onMouseLeave={() => setTooltip(null)}
                                    style={{
                                        default: {
                                            fill: isHighlighted ? '#10b981' : '#1e293b',
                                            stroke: isHighlighted ? '#34d399' : '#334155',
                                            strokeWidth: 0.5,
                                            outline: 'none',
                                        },
                                        hover: {
                                            fill: isHighlighted ? '#6ee7b7' : '#1e293b',
                                            stroke: isHighlighted ? '#34d399' : '#334155',
                                            strokeWidth: 0.5,
                                            outline: 'none',
                                            cursor: isHighlighted ? 'pointer' : 'default',
                                        },
                                        pressed: {
                                            fill: isHighlighted ? '#6ee7b7' : '#1e293b',
                                            stroke: isHighlighted ? '#34d399' : '#334155',
                                            strokeWidth: 0.5,
                                            outline: 'none',
                                        },
                                    }}
                                />
                            );
                        })
                    }
                </Geographies>
            </ComposableMap>

            {tooltip && (
                <div
                    className="absolute z-50 pointer-events-none bg-slate-900 border border-slate-700 px-3 py-2 flex items-center gap-2 shadow-lg whitespace-nowrap"
                    style={{ left: tooltip.x + 14, top: tooltip.y + 14 }}
                >
                    <CountryFlag countryCode={tooltip.row.countryCode ?? ''} className="text-lg" />
                    <span className="text-sm text-white font-medium">{tooltip.row.country}</span>
                    <span className="text-sm text-emerald-400">{tooltip.row.pilots ?? 0} pilots</span>
                </div>
            )}

            <div className="relative flex flex-wrap items-center justify-between gap-x-6 gap-y-2 mt-4 pt-4 border-t border-slate-700 text-xs text-slate-400">
                <div className="flex flex-wrap items-center gap-x-6 gap-y-2">
                    <div className="flex items-center gap-2">
                        <span className="inline-block h-3 w-3 bg-emerald-500 border border-emerald-400" />
                        Countries with pilots
                    </div>
                    <div className="flex items-center gap-2">
                        <span className="inline-block h-3 w-3 bg-slate-800 border border-slate-700" />
                        No pilots yet
                    </div>
                </div>
                <div className="text-slate-500">Hover a highlighted country for pilot count.</div>
            </div>
        </div>
    );
}

export default WorldMap;
