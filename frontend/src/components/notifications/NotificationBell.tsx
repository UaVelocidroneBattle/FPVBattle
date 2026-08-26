import { useEffect, useRef, useState } from "react";
import { Bell, Info, Medal, Shield, Trophy, type LucideIcon } from "lucide-react";
import { useNotificationsStore } from "@/store/notificationsStore";
import { formatDate } from "@/lib/utils";
import { useCups } from "@/hooks/useCups";
import type { NotificationType } from "@/api/client";

const notificationIcons: Record<NotificationType, LucideIcon> = {
    General: Info,
    Achievement: Medal,
    CompetitionResults: Trophy,
    LeagueUpdate: Shield,
};

function formatTimestamp(value: string) {
    const date = new Date(value);
    const isToday = date.toDateString() === new Date().toDateString();

    if (!isToday) return formatDate(value);

    const hours = String(date.getHours()).padStart(2, "0");
    const minutes = String(date.getMinutes()).padStart(2, "0");

    return `${hours}:${minutes}`;
}

function NotificationBell() {
    const unreadCount = useNotificationsStore((state) => state.unreadCount);
    const items = useNotificationsStore((state) => state.items);
    const state = useNotificationsStore((state) => state.state);
    const { findCup } = useCups();
    const [open, setOpen] = useState(false);
    const containerRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if (!open) return;

        const closeOnOutsideClick = (event: MouseEvent) => {
            if (!containerRef.current?.contains(event.target as Node)) {
                setOpen(false);
            }
        };
        document.addEventListener("mousedown", closeOnOutsideClick);
        return () => document.removeEventListener("mousedown", closeOnOutsideClick);
    }, [open]);

    useEffect(() => {
        if (open) {
            useNotificationsStore.getState().fetchItems();
        }
    }, [open]);

    return (
        <div ref={containerRef} className="relative flex items-center">
            <button
                onClick={() => setOpen(isOpen => !isOpen)}
                aria-label={unreadCount > 0 ? `Notifications, ${unreadCount} unread` : "Notifications"}
                className="relative flex h-8 w-8 items-center justify-center rounded-full text-slate-300 transition-colors hover:text-emerald-400"
            >
                <Bell className="h-5 w-5" />
                {unreadCount > 0 && (
                    <span className="absolute -right-0.5 -top-0.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-emerald-500 px-1 text-[10px] font-semibold text-slate-900">
                        {unreadCount > 9 ? "9+" : unreadCount}
                    </span>
                )}
            </button>

            {open && (
                <div className="absolute right-0 top-full z-20 mt-2 w-80 border border-slate-700 bg-slate-900 shadow-lg">
                    <div className="flex items-center justify-between border-b border-slate-700 px-4 py-3">
                        <span className="text-sm font-semibold text-slate-200">Notifications</span>
                        {unreadCount > 0 && (
                            <button
                                onClick={() => useNotificationsStore.getState().markAllRead()}
                                className="text-xs text-slate-400 transition-colors hover:text-emerald-400"
                            >
                                Mark all read
                            </button>
                        )}
                    </div>

                    <div className="scrollbar-slim max-h-80 overflow-y-auto">
                        {state === "Loading" && items.length === 0 && (
                            <p className="px-4 py-6 text-center text-sm text-slate-400">Loading…</p>
                        )}

                        {state === "Error" && (
                            <p className="px-4 py-6 text-center text-sm text-slate-400">Could not load notifications.</p>
                        )}

                        {state === "Loaded" && items.length === 0 && (
                            <p className="px-4 py-6 text-center text-sm text-slate-400">Nothing here yet.</p>
                        )}

                        {items.map((item) => {
                            const Icon = notificationIcons[item.type];
                            const cupName = item.cupId ? (findCup(item.cupId)?.name ?? item.cupId) : null;

                            return (
                                <button
                                    key={item.id}
                                    onClick={() => useNotificationsStore.getState().markRead([item.id])}
                                    className="flex w-full items-start gap-3 border-b border-slate-800 px-4 py-3 text-left transition-colors last:border-b-0 hover:bg-slate-800"
                                >
                                    <Icon
                                        className={`mt-0.5 h-4 w-4 flex-none ${item.readOn ? "text-slate-500" : "text-emerald-400"}`}
                                        aria-hidden
                                    />
                                    <span className="min-w-0 flex-1">
                                        <span className={`block text-sm ${item.readOn ? "text-slate-400" : "text-slate-200"}`}>
                                            {item.text}
                                        </span>
                                        <span className="mt-1 flex items-center gap-2 text-xs text-slate-500">
                                            <span>{formatTimestamp(item.createdOn)}</span>
                                            {cupName && (
                                                <span className="rounded-sm border border-slate-700 px-1.5 py-px text-[10px] uppercase tracking-wide text-slate-400">
                                                    {cupName}
                                                </span>
                                            )}
                                        </span>
                                    </span>
                                    {!item.readOn && (
                                        <span className="mt-1.5 h-2 w-2 flex-none rounded-full bg-emerald-500" aria-hidden />
                                    )}
                                </button>
                            );
                        })}
                    </div>
                </div>
            )}
        </div>
    );
}

export default NotificationBell;
