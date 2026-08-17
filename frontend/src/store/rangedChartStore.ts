import { create, StoreApi, UseBoundStore } from 'zustand';
import { LoadingStates } from '../lib/loadingStates';

/**
 * One time scale the reader can put a chart on.
 */
export interface RangeOption {
    /** Stable identifier, used for the button key and to cache the range's data. */
    key: string;
    label: string;
    /**
     * Window size sent to the API — days or months, whichever the chart's endpoint
     * counts in. `null` asks for the whole history.
     */
    size: number | null;
}

interface RangedChartState<TData> {
    ranges: readonly RangeOption[];
    /** The range the reader has selected — highlighted in the picker straight away. */
    range: RangeOption;
    data: TData | null;
    /**
     * The range `data` actually holds, which trails `range` while the new one loads.
     * Charts key their rendering off this so they redraw when the numbers change
     * rather than when the button is pressed.
     */
    dataRange: string | null;
    loadingState: LoadingStates;
}

interface RangedChartActions {
    /** Loads the default range. Safe to call on every mount — later calls are ignored. */
    load: () => Promise<void>;
    selectRange: (key: string) => Promise<void>;
}

export type RangedChartStore<TData> = RangedChartState<TData> & RangedChartActions;

/**
 * Builds a store for a chart the reader can rescale.
 *
 * Every chart on the statistics dashboard fetches one window at a time rather than
 * the whole history, so they all need the same three things: which range is showing,
 * its data, and whether it has arrived yet. Writing that per chart put the same
 * twenty lines in three files, so it lives here once and each chart supplies only
 * what is genuinely its own — its ranges and how to fetch one.
 *
 * @param ranges Scales offered to the reader; the first is the default.
 * @param fetchRange Fetches one window. Receives the selected range's `size`.
 */
export function createRangedChartStore<TData>(
    ranges: readonly RangeOption[],
    fetchRange: (size: number | null) => Promise<TData | undefined>
): UseBoundStore<StoreApi<RangedChartStore<TData>>> {
    // Ranges are immutable once fetched, so a range the reader returns to is free.
    // Kept out of the store: it is a cache, not something a component renders.
    const cache = new Map<string, TData>();

    return create<RangedChartStore<TData>>()((set, get) => {
        const show = async (range: RangeOption) => {
            const cached = cache.get(range.key);

            if (cached) {
                set({ range, data: cached, dataRange: range.key, loadingState: 'Loaded' });
                return;
            }

            // The previous range stays on screen while the new one loads, so the
            // page never collapses to a spinner and jumps back.
            set({ range, loadingState: 'Loading' });

            try {
                const data = await fetchRange(range.size);

                if (!data) throw new Error('No data');

                cache.set(range.key, data);

                // The reader may have moved on while this was in flight; a slow
                // answer must not overwrite the range they are now looking at.
                if (get().range.key !== range.key) return;

                set({ data, dataRange: range.key, loadingState: 'Loaded' });
            } catch {
                if (get().range.key !== range.key) return;

                set({ loadingState: 'Error' });
            }
        };

        return {
            ranges,
            range: ranges[0],
            data: null,
            dataRange: null,
            loadingState: 'Idle',

            load: async () => {
                if (get().loadingState !== 'Idle') return;

                await show(get().range);
            },

            selectRange: async (key: string) => {
                const range = ranges.find((option) => option.key === key);

                if (!range || range.key === get().range.key) return;

                await show(range);
            },
        };
    });
}
