import { useState } from "react";
import { Link } from "react-router-dom";
import { getApiProfilePilotLookup } from "@/api/client";
import type { PilotLookupModel, ProfileModel } from "@/api/client";
import { useProfileStore } from "@/store/profileStore";
import CountryFlag from "@/components/ui/CountryFlag";
import { formatDate } from "@/lib/utils";

interface PilotBindingSectionProps {
    profile: ProfileModel;
}

function LinkedPilotCard({ profile }: { profile: ProfileModel }) {
    const pilot = profile.pilot!;

    return (
        <div>
            <div className="flex items-center gap-3">
                <CountryFlag countryCode={pilot.country} />
                <Link
                    to={`/statistics/profile/${encodeURIComponent(pilot.name)}`}
                    className="text-lg font-semibold text-emerald-400 hover:underline"
                >
                    {pilot.name}
                </Link>
            </div>
            <p className="mt-4 text-xs text-slate-500">
                Linked accounts cannot be changed here — contact an admin if this is not you.
            </p>
        </div>
    );
}

function PendingClaimCard({ profile }: { profile: ProfileModel }) {
    const claim = profile.pendingClaim!;

    if (claim.isExpired) {
        return (
            <div className="mb-4 border border-amber-700/50 bg-amber-950/30 p-4 text-sm">
                Your claim for{" "}
                <span className="font-semibold text-emerald-400">{claim.pilotName}</span> expired before a
                verification race was flown. You can claim again below.
            </div>
        );
    }

    return (
        <div className="border border-emerald-700/50 bg-emerald-950/30 p-4 text-sm">
            <p>
                Almost there! To verify that{" "}
                <span className="font-semibold text-emerald-400">{claim.pilotName}</span> is you, fly the
                daily track in any class. Your account will be linked automatically as soon as your result
                appears.
            </p>
            <p className="mt-2 text-slate-400">
                The claim expires on {new Date(claim.expiresAt).toLocaleString()}.
            </p>
            <button
                onClick={() => useProfileStore.getState().cancelClaim()}
                className="mt-3 text-slate-400 transition-colors hover:text-emerald-400"
            >
                Cancel claim
            </button>
        </div>
    );
}

function ClaimForm() {
    const claimError = useProfileStore((state) => state.claimError);
    const [pilotName, setPilotName] = useState("");
    const [lookup, setLookup] = useState<PilotLookupModel | null>(null);
    const [searching, setSearching] = useState(false);
    const [claiming, setClaiming] = useState(false);
    const [lookupFailed, setLookupFailed] = useState(false);

    const search = async () => {
        const name = pilotName.trim();
        if (!name || searching) return;

        setSearching(true);
        setLookupFailed(false);
        useProfileStore.getState().clearClaimError();
        try {
            const response = await getApiProfilePilotLookup({ query: { pilotName: name } });
            if (response.data) {
                setLookup(response.data);
            } else {
                setLookupFailed(true);
            }
        } catch {
            setLookupFailed(true);
        } finally {
            setSearching(false);
        }
    };

    const claim = async () => {
        if (claiming) return;

        setClaiming(true);
        try {
            await useProfileStore.getState().claimPilot(pilotName.trim());
        } finally {
            setClaiming(false);
        }
    };

    return (
        <div>
            <p className="text-sm text-slate-400">
                Enter your Velocidrone name to link your account with your pilot. The name is case
                sensitive, so type it exactly as it is in the Velocidrone.
            </p>

            <div className="mt-3 flex gap-2">
                <input
                    value={pilotName}
                    onChange={(e) => {
                        setPilotName(e.target.value);
                        setLookup(null);
                        setLookupFailed(false);
                    }}
                    onKeyDown={(e) => e.key === "Enter" && search()}
                    maxLength={128}
                    placeholder="Velocidrone name"
                    className="w-full max-w-xs border border-slate-600 bg-slate-900 px-3 py-2 text-sm placeholder:text-slate-500 focus:border-emerald-400 focus:outline-none"
                />
                <button
                    onClick={search}
                    disabled={!pilotName.trim() || searching}
                    className="border border-slate-600 px-4 text-sm transition-colors hover:border-emerald-400 hover:text-emerald-400 disabled:opacity-50 disabled:hover:border-slate-600 disabled:hover:text-inherit"
                >
                    Find
                </button>
            </div>

            {lookup && (
                <div className="mt-4 border border-slate-700 bg-slate-900 p-4 text-sm">
                    {lookup.found ? (
                        lookup.alreadyLinked ? (
                            <p className="text-red-400">
                                Pilot <span className="font-semibold">{lookup.name}</span> is already linked to
                                another account. If this is your pilot, contact an admin.
                            </p>
                        ) : (
                            <>
                                <h3 className="text-sm font-semibold uppercase tracking-wide text-slate-400">
                                    Found
                                </h3>
                                <div className="mt-2 flex items-center gap-3">
                                    <CountryFlag countryCode={lookup.country ?? ""} />
                                    <span className="font-semibold text-emerald-400">{lookup.name}</span>
                                    <div className="h-4 w-px bg-slate-600" />
                                    <span className="text-slate-400">
                                        last race: {formatDate(lookup.lastRaceDate)}
                                    </span>
                                </div>
                                <p className="mt-3 text-slate-400">
                                    To confirm this pilot is you, you will need to fly the current daily track
                                    after claiming.
                                </p>
                            </>
                        )
                    ) : (
                        <>
                            <h3 className="text-sm font-semibold uppercase tracking-wide text-slate-400">
                                Not found
                            </h3>
                            <p className="mt-2">
                                No pilot named{" "}
                                <span className="font-semibold text-emerald-400">{pilotName.trim()}</span> has
                                raced with us yet.
                            </p>
                            <p className="mt-3 text-slate-400">
                                Claim the name and fly the daily track in Velocidrone. Your pilot will be
                                created and linked automatically as soon as your first result appears.
                            </p>
                        </>
                    )}

                    {(!lookup.found || !lookup.alreadyLinked) && (
                        <button
                            onClick={claim}
                            disabled={claiming}
                            className="mt-5 border border-emerald-500 px-4 py-2 text-emerald-400 transition-colors hover:bg-emerald-500/10 disabled:opacity-50 disabled:hover:bg-transparent"
                        >
                            This is me
                        </button>
                    )}
                </div>
            )}

            {lookupFailed && (
                <p className="mt-3 text-sm text-red-400">
                    Could not check the name right now. Please try again.
                </p>
            )}

            {claimError && <p className="mt-3 text-sm text-red-400">{claimError}</p>}
        </div>
    );
}

/**
 * The "My pilot" section of the profile page: shows the linked pilot,
 * the pending fly-to-verify claim, or the claim form.
 */
function PilotBindingSection({ profile }: PilotBindingSectionProps) {
    if (profile.pilot) return <LinkedPilotCard profile={profile} />;

    const hasActiveClaim = profile.pendingClaim && !profile.pendingClaim.isExpired;

    return (
        <div>
            {profile.pendingClaim && <PendingClaimCard profile={profile} />}
            {!hasActiveClaim && <ClaimForm />}
        </div>
    );
}

export default PilotBindingSection;
