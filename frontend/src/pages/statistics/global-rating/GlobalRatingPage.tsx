import { useEffect } from "react";
import { ArrowDown, ArrowUp } from "lucide-react";
import { useGlobalRatingStore, PilotRatingModel, LeagueSettingsModel } from "@/store/globalRatingStore";
import { Spinner } from "@/components/ui/spinner";
import PilotWithAvatar from "@/components/PilotWithAvatar";
import { formatDate } from "@/lib/utils";

const CUP_ID = "open-class";


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

function RatingRow({ pilot, leagueColors, showLeague }: { pilot: PilotRatingModel; leagueColors: Map<string, string>; showLeague: boolean }) {
    const leagueColor = pilot.league ? leagueColors.get(pilot.league) : undefined;

    return (
        <li className="px-3 py-6 hover:bg-slate-700/30 transition-colors duration-150">
            <div className="flex items-center gap-4">
                <div className="relative w-8 flex-shrink-0">
                    <span className="font-bold tabular-nums text-lg sm:text-2xl text-slate-400">
                        {String(pilot.rank).padStart(2, "0")}
                    </span>
                    <RankChange change={pilot.rankChange} />
                </div>

                <div className="flex-1 min-w-0">
                    <PilotWithAvatar name={pilot.pilotName} countryCode={pilot.country ?? null} />
                </div>

                {showLeague && (
                    <div
                        className="w-20 sm:w-28 flex-shrink-0 pr-4 sm:pr-6 text-sm font-medium text-right truncate"
                        style={{ color: leagueColor || undefined }}
                    >
                        {pilot.league ?? "—"}
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
    | { kind: "pilot"; key: string; pilot: PilotRatingModel };

function RatingsTable({ items, leagueColors, showLeague }: { items: RatingListItem[]; leagueColors: Map<string, string>; showLeague: boolean }) {
    return (
        <div className="overflow-hidden -mx-6 sm:mx-0">
            <div className="px-3 py-3 border-b border-slate-700/50 flex items-center gap-4">
                <div className="w-8 flex-shrink-0" />
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
                        <RatingRow key={item.key} pilot={item.pilot} leagueColors={leagueColors} showLeague={showLeague} />
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
    const descriptors = leagueSettings.descriptors ?? [];
    const orderByName = new Map(descriptors.map((d) => [d.name, d.order ?? 0]));
    const colorByName = new Map(descriptors.map((d) => [d.name, d.color ?? undefined]));
    const othersName = leagueSettings.othersName ?? "Others";

    const groups = new Map<string, PilotRatingModel[]>();
    for (const pilot of ratings) {
        const name = pilot.league ?? othersName;
        if (!groups.has(name)) groups.set(name, []);
        groups.get(name)!.push(pilot);
    }

    return Array.from(groups.entries())
        .map(([name, pilots]) => ({
            name,
            color: colorByName.get(name),
            order: orderByName.get(name) ?? Number.MAX_SAFE_INTEGER,
            pilots,
        }))
        .sort((a, b) => a.order - b.order);
}

function buildRatingListItems(zones: LeagueZone[]): RatingListItem[] {
    return zones.flatMap((zone) => [
        { kind: "divider", key: `zone-${zone.name}`, name: zone.name, color: zone.color } as const,
        ...zone.pilots.map((pilot) => ({ kind: "pilot", key: String(pilot.pilotId), pilot }) as const),
    ]);
}

function GlobalRatingPage() {
    const data = useGlobalRatingStore((state) => state.data);
    const loadingState = useGlobalRatingStore((state) => state.loadingState);

    useEffect(() => {
        const { fetchRatings } = useGlobalRatingStore.getState();
        fetchRatings(CUP_ID);
    }, []);

    const showLeagues = data?.leagueSettings.enabled ?? false;
    const leagueColors = new Map<string, string>(
        data?.leagueSettings.descriptors
            ?.filter((d): d is typeof d & { color: string } => !!d.color)
            .map((d) => [d.name, d.color]) ?? []
    );
    const ratingListItems: RatingListItem[] = data
        ? showLeagues
            ? buildRatingListItems(buildLeagueZones(data.ratings, data.leagueSettings))
            : data.ratings.map((pilot) => ({ kind: "pilot", key: String(pilot.pilotId), pilot }) as const)
        : [];

    return (
        <div className="space-y-6">
            <div>
                <h2 className="text-xl font-semibold text-white mb-1">Global Rating</h2>
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
                    <RatingsTable items={ratingListItems} leagueColors={leagueColors} showLeague={showLeagues} />

                    {data.droppedOutPilots.length > 0 && (
                        <div className="pt-8">
                            <h3 className="text-sm font-semibold uppercase tracking-wider text-slate-500 mb-3">
                                Dropped out
                            </h3>
                            <ul className="divide-y divide-slate-700/50 -mx-6 sm:mx-0">
                                {data.droppedOutPilots.map((pilot) => (
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

export default GlobalRatingPage;
