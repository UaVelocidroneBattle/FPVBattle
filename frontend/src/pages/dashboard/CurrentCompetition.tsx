import { useState } from "react";
import { Check, Copy, ExternalLink } from "lucide-react";
import { Link } from "react-router-dom";
import { DashboardModel } from "@/api/client";

interface ICurrentCompetitionProps {
    cupId: string;
    dashboard: DashboardModel;
    selectedDate: string | null;
    onDateChange: (date: string | null) => void;
}

const today = new Date().toISOString().split('T')[0];

function getClassLabel(cupId: string): string {
    return cupId === 'whoop-class' ? 'Whoop Class' : 'Open Class';
}

function getSeasonName(selectedDate: string | null): string {
    const date = selectedDate ? new Date(selectedDate + 'T00:00:00') : new Date();
    return date.toLocaleDateString('en-US', { month: 'long', year: 'numeric' });
}


function Separator() {
    return <span className="hidden md:inline text-slate-600 select-none">|</span>;
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
    return (
        <span className="flex items-start gap-1.5">
            <span className="text-emerald-400 shrink-0">{label}:</span>
            {children}
        </span>
    );
}

function CurrentCompetition({ cupId, dashboard, selectedDate, onDateChange }: ICurrentCompetitionProps) {
    const [copied, setCopied] = useState(false);

    const handleDateChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const value = e.target.value;
        onDateChange(value === today || value === '' ? null : value);
    };

    const copyTrackName = async (trackName: string) => {
        try {
            await navigator.clipboard.writeText(trackName);
            setCopied(true);
            setTimeout(() => setCopied(false), 2000);
        } catch (err) {
            console.error('Failed to copy:', err);
        }
    };

    const datePickerValue = selectedDate ?? today;

    return (
        <div className="px-4 py-3 flex flex-col md:flex-row md:items-center gap-2 md:gap-3 text-base">
            <span className="text-xs font-semibold uppercase tracking-wider bg-emerald-400/10 text-emerald-400 border border-emerald-400/20 px-2.5 py-1 shrink-0 text-center">
                {getClassLabel(cupId)}
            </span>

            {dashboard.competition == null ? (
                <>
                    <Separator />
                    <span className="text-sm text-slate-400">Nothing scheduled</span>
                </>
            ) : (
                <>
                    <Separator />

                    <Field label="Track">
                        <span>
                            <span className="text-slate-400">{dashboard.competition.mapName} - </span>
                            <button
                                onClick={() => copyTrackName(dashboard.competition!.trackName)}
                                className="inline-flex items-center gap-1.5 text-white hover:text-emerald-400 transition-colors group"
                                title="Copy track name"
                            >
                                {dashboard.competition.trackName}
                                {copied
                                    ? <Check className="h-4 w-4 text-emerald-400 shrink-0" />
                                    : <Copy className="h-4 w-4 text-slate-600 group-hover:text-emerald-400 shrink-0 transition-colors" />
                                }
                            </button>
                        </span>
                    </Field>

                    <Separator />

                    <Field label="Quad">
                        <span className={dashboard.competition.quadOfTheDay ? "text-white" : "text-slate-500"}>
                            {dashboard.competition.quadOfTheDay ?? "—"}
                        </span>
                    </Field>

                    <Separator />

                    <Field label="Season">
                        <span className="text-white">{getSeasonName(selectedDate)}</span>
                    </Field>

                    <Separator />

                    <Link
                        className="text-sm text-slate-400 hover:text-emerald-400 transition-colors inline-flex items-center gap-1.5"
                        to={`https://www.velocidrone.com/leaderboard/${dashboard.competition.mapId}/${dashboard.competition.trackId}/All`}
                        target="_blank"
                    >
                        Velocidrone leaderboard
                        <ExternalLink className="w-3.5 h-3.5" />
                    </Link>
                </>
            )}

            <div className="hidden md:block flex-1" />

            <div className="flex items-center gap-3 shrink-0">
                {selectedDate && (
                    <button
                        onClick={() => onDateChange(null)}
                        className="text-sm text-emerald-400 hover:text-emerald-300 transition-colors whitespace-nowrap"
                    >
                        Back to today
                    </button>
                )}
                <input
                    type="date"
                    value={datePickerValue}
                    max={today}
                    onChange={handleDateChange}
                    className="bg-slate-700/50 border border-slate-600 text-slate-200 text-sm px-3 py-1 focus:outline-none focus:ring-1 focus:ring-emerald-400 focus:border-emerald-400 [color-scheme:dark]"
                />
            </div>
        </div>
    );
}

export default CurrentCompetition;
