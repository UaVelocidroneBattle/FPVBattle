import { useEffect, useMemo } from "react";
import { Link, Navigate, useParams } from "react-router-dom";
import { ArrowDown, ArrowUp } from "lucide-react";
import { useGlobalRatingStore, PilotRatingModel, LeagueSettingsModel } from "@/store/globalRatingStore";
import { Spinner } from "@/components/ui/spinner";
import PilotWithAvatar from "@/components/PilotWithAvatar";
import { useCups } from "@/hooks/useCups";
import { useHighlightedPilot } from "@/hooks/useHighlightedPilot";
import CountryFilter, { countryOptionsOf } from "./CountryFilter";
import { useUrlCountry } from "./useUrlCountry";
import { CupModel } from "@/store/cupsStore";
import { formatDate } from "@/lib/utils";


function RankChange({ change }: { change: number | null }) {
    if (change === null)
        return (
            <span className="absolute top-full mt-0.5 left-0 text-[10px] font-bold text-amber-400 tracking-wide">
                NEW
            </span>
        );
    if (change === 0) return null;
    const improved = change < 0;
    const Icon = improved ? ArrowUp : ArrowDown;
    return (
        <span className={`absolute top-full mt-0.5 left-0 flex items-center gap-0.5 text-xs font-medium ${improved ? "text-emerald-400" : "text-red-400"}`}>
            <Icon className="h-3 w-3" />
            {Math.abs(change)}
        </span>
    );
}

function GapChange({ change }: { change: number }) {
    if (change === 0) return null;
    const improved = change < 0;
    return (
        <span className={`absolute top-full mt-0.5 right-0 text-xs font-medium ${improved ? "text-emerald-400" : "text-red-400"}`}>
            {improved ? "−" : "+"}
            {Math.abs(change).toFixed(2)}%
        </span>
    );
}

function formatGap(value: number | null): string {
    if (value === null) return "—";
    const prefix = value > 0 ? "+" : "";
    return `${prefix}${value.toFixed(2)}%`;
}

const formatRank = (rank: number) => String(rank).padStart(2, "0");

/**
 * Marks the signed in user's own row, matching the competition leaderboards.
 * The list draws its separators with `divide-y`, whose selector outranks a
 * plain border colour, so the emerald lines recolour the dividers instead:
 * this row's own top one, and the next row's, which sits under it.
 */
const highlightClasses = "bg-emerald-500/10 !border-t-emerald-400/40 [&+li]:!border-t-emerald-400/40";

/**
 * Position within a country leaderboard keeps the global rank alongside it,
 * e.g. "01 (15)". Without a country only the global rank is shown.
 */
function Position({ pilot, localRank }: { pilot: PilotRatingModel; localRank?: number }) {
    return (
        <span className="font-bold tabular-nums whitespace-nowrap text-lg sm:text-2xl text-slate-400">
            {formatRank(localRank ?? pilot.rank)}
            {localRank !== undefined && (
                <>
                    {" "}
                    <span className="text-sm font-medium text-slate-500">({formatRank(pilot.rank)})</span>
                </>
            )}
        </span>
    );
}

function RatingRow({ pilot, localRank, leagueColors, showLeague, othersName, isHighlighted }: { pilot: PilotRatingModel; localRank?: number; leagueColors: Map<string, string>; showLeague: boolean; othersName: string; isHighlighted: boolean }) {
    const leagueColor = pilot.league ? leagueColors.get(pilot.league) : undefined;

    return (
        <li className={`px-3 py-6 hover:bg-slate-700/30 transition-colors duration-150 ${isHighlighted ? highlightClasses : ""}`}>
            <div className="flex items-center gap-4">
                <div className={`relative flex-shrink-0 ${positionColumnWidth(localRank !== undefined)}`}>
                    <Position pilot={pilot} localRank={localRank} />
                    <RankChange change={pilot.rankChange} />
                </div>

                <div className="flex-1 min-w-0">
                    <PilotWithAvatar name={pilot.pilotName} countryCode={pilot.country ?? null} />
                </div>

                {showLeague && (
                    <div
                        className={`w-20 sm:w-28 flex-shrink-0 pr-4 sm:pr-6 text-sm font-medium text-right truncate ${leagueColor ? "" : "text-slate-500"}`}
                        style={{ color: leagueColor || undefined }}
                    >
                        {pilot.league ?? othersName}
                    </div>
                )}

                <div className="relative w-20 sm:w-24 flex-shrink-0 text-right">
                    <span className="text-lg font-semibold text-slate-300 tabular-nums">
                        {formatGap(pilot.averageGapPercent)}
                    </span>
                    <GapChange change={pilot.averageGapChange ?? 0} />
                </div>
            </div>
        </li>
    );
}

function ZoneDivider({ name, color }: { name: string; color?: string }) {
    return (
        <li className="sticky top-0 z-10 bg-slate-800/95 backdrop-blur-sm px-3 py-2 text-center">
            <span className="text-xs font-semibold uppercase tracking-wider text-emerald-400" style={{ color }}>
                {name} zone
            </span>
        </li>
    );
}

type RatingListItem =
    | { kind: "divider"; key: string; name: string; color?: string }
    | { kind: "pilot"; key: string; pilot: PilotRatingModel; localRank?: number };

/**
 * The position column is sized to its content, so the pilot beside it sits
 * just as close whether one rank is shown or two.
 */
function positionColumnWidth(withLocalRank: boolean): string {
    return withLocalRank ? "w-16" : "w-8";
}

function RatingsTable({ items, leagueColors, showLeague, othersName, highlightPilotName }: { items: RatingListItem[]; leagueColors: Map<string, string>; showLeague: boolean; othersName: string; highlightPilotName: string | null }) {
    const withLocalRanks = items.some((item) => item.kind === "pilot" && item.localRank !== undefined);

    return (
        <div className="overflow-hidden -mx-6 sm:mx-0">
            <div className="px-3 py-3 border-b border-slate-700/50 flex items-center gap-4">
                <div className={`flex-shrink-0 ${positionColumnWidth(withLocalRanks)}`} />
                <div className="flex-1 text-xs font-semibold uppercase tracking-wider text-slate-500">Pilot</div>
                {showLeague && (
                    <div className="w-20 sm:w-28 flex-shrink-0 pr-4 sm:pr-6 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">League</div>
                )}
                <div className="w-20 sm:w-24 flex-shrink-0 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">Gap</div>
            </div>
            <ul className="divide-y divide-slate-700/50">
                {items.map((item) =>
                    item.kind === "divider" ? (
                        <ZoneDivider key={item.key} name={item.name} color={item.color} />
                    ) : (
                        <RatingRow key={item.key} pilot={item.pilot} localRank={item.localRank} leagueColors={leagueColors} showLeague={showLeague} othersName={othersName} isHighlighted={item.pilot.pilotName === highlightPilotName} />
                    )
                )}
            </ul>
        </div>
    );
}

interface LeagueZone {
    name: string;
    color?: string;
    pilots: PilotRatingModel[];
}

function buildLeagueZones(ratings: PilotRatingModel[], leagueSettings: LeagueSettingsModel): LeagueZone[] {
    const descriptors = [...(leagueSettings.descriptors ?? [])].sort((a, b) => (a.order ?? 0) - (b.order ?? 0));
    const othersName = leagueSettings.othersName ?? "Others";

    const zones: LeagueZone[] = [];
    let index = 0;
    for (const descriptor of descriptors) {
        if (index >= ratings.length) break;

        const size = descriptor.size || ratings.length - index;
        const pilots = ratings.slice(index, index + size);
        if (pilots.length > 0) {
            zones.push({ name: descriptor.name, color: descriptor.color ?? undefined, pilots });
        }
        index += pilots.length;
    }

    const remaining = ratings.slice(index);
    if (remaining.length > 0) {
        zones.push({ name: othersName, pilots: remaining });
    }

    return zones;
}

function buildRatingListItems(zones: LeagueZone[]): RatingListItem[] {
    return zones.flatMap((zone) => [
        { kind: "divider", key: `zone-${zone.name}`, name: zone.name, color: zone.color } as const,
        ...zone.pilots.map((pilot) => ({ kind: "pilot", key: String(pilot.pilotId), pilot }) as const),
    ]);
}

/**
 * A single country is a leaderboard of its own: no league zones, positions
 * numbered from one, with the global rank kept alongside each of them.
 */
function buildCountryListItems(ratings: PilotRatingModel[], countryCode: string): RatingListItem[] {
    return ratings
        .filter((pilot) => pilot.country === countryCode)
        .map((pilot, index) => ({ kind: "pilot", key: String(pilot.pilotId), pilot, localRank: index + 1 }) as const);
}

function CupRating({ cup }: { cup: CupModel }) {
    const data = useGlobalRatingStore((state) => state.data);
    const loadingState = useGlobalRatingStore((state) => state.loadingState);
    const highlightPilotName = useHighlightedPilot();

    useEffect(() => {
        const { fetchRatings } = useGlobalRatingStore.getState();
        fetchRatings(cup.id);
    }, [cup.id]);

    const countryOptions = useMemo(() => countryOptionsOf(data?.ratings ?? []), [data]);
    const [country, setCountry] = useUrlCountry(countryOptions);

    const showLeagues = data?.leagueSettings.enabled ?? false;
    const othersName = data?.leagueSettings.othersName ?? "Others";
    const leagueColors = new Map<string, string>(
        data?.leagueSettings.descriptors
            ?.filter((d): d is typeof d & { color: string } => !!d.color)
            .map((d) => [d.name, d.color]) ?? []
    );
    const matchesCountry = (pilot: PilotRatingModel) => !country.code || pilot.country === country.code;
    const ratingListItems: RatingListItem[] = !data
        ? []
        : country.code
            ? buildCountryListItems(data.ratings, country.code)
            : showLeagues
                ? buildRatingListItems(buildLeagueZones(data.ratings, data.leagueSettings))
                : data.ratings.map((pilot) => ({ kind: "pilot", key: String(pilot.pilotId), pilot }) as const);
    const droppedOutPilots = data?.droppedOutPilots.filter(matchesCountry) ?? [];

    return (
        <div className="space-y-6">
            <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between sm:gap-4">
                <div>
                    <div className="flex flex-wrap items-baseline gap-x-3 mb-1">
                        <h2 className="text-xl font-semibold text-white">{cup.name} global rating</h2>
                        {/* Grouped so the separator never wraps onto a line of its own. */}
                        <span className="flex items-baseline gap-3">
                            <span className="text-slate-600">·</span>
                            <Link
                                to="/guide/global-rating"
                                className="shrink-0 text-sm text-slate-400 hover:text-emerald-400 transition-colors"
                            >
                                How does it work?
                            </Link>
                        </span>
                    </div>
                    {data && (
                        <p className="text-slate-400 text-sm flex items-center gap-2">
                            <span>Last update: {formatDate(data.calculatedOn)}</span>
                            <span className="text-slate-600">·</span>
                            <span className="text-emerald-400">new: {data.ratings.filter(p => p.rankChange === null).length}</span>
                            <span className="text-slate-600">·</span>
                            <span className="text-red-400">dropped: {data.droppedOutPilots.length}</span>
                        </p>
                    )}
                </div>

                {loadingState === "Loaded" && data && (
                    <CountryFilter options={countryOptions} value={country} onChange={setCountry} />
                )}
            </div>

            {loadingState === "Loading" && (
                <div className="flex justify-center py-12">
                    <Spinner />
                </div>
            )}

            {loadingState === "Error" && (
                <p className="text-red-400 text-center py-8">Failed to load ratings</p>
            )}

            {loadingState === "Loaded" && data && (
                <>
                    <RatingsTable items={ratingListItems} leagueColors={leagueColors} showLeague={showLeagues} othersName={othersName} highlightPilotName={highlightPilotName} />

                    {droppedOutPilots.length > 0 && (
                        <div className="pt-8">
                            <h3 className="text-sm font-semibold uppercase tracking-wider text-slate-500 mb-3">
                                Dropped out
                            </h3>
                            <ul className="divide-y divide-slate-700/50 -mx-6 sm:mx-0">
                                {droppedOutPilots.map((pilot) => (
                                    <li key={pilot.pilotId} className="px-3 py-3">
                                        <PilotWithAvatar name={pilot.pilotName} countryCode={pilot.country} />
                                    </li>
                                ))}
                            </ul>
                        </div>
                    )}
                </>
            )}
        </div>
    );
}

function GlobalRatingPage() {
    const { cupId } = useParams();
    const { loadingState, defaultCup, findCup } = useCups();

    if (loadingState === "Error")
        return <p className="text-red-400 text-center py-8">Failed to load cups</p>;

    if (loadingState !== "Loaded")
        return (
            <div className="flex justify-center py-12">
                <Spinner />
            </div>
        );

    const cup = findCup(cupId);

    // An unknown or missing cup lands on the first configured one.
    if (!cup)
        return defaultCup
            ? <Navigate to={`/global-rating/${defaultCup.id}`} replace />
            : <p className="text-slate-400 text-center py-8">No cups are running right now</p>;

    // Keyed by cup so switching cups remounts with a clean loading state.
    return <CupRating key={cup.id} cup={cup} />;
}

export default GlobalRatingPage;
