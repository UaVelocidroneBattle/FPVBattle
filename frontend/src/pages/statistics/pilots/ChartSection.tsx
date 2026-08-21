import { ReactNode, useEffect } from 'react';
import { StoreApi, UseBoundStore } from 'zustand';
import { useShallow } from 'zustand/shallow';
import { RangedChartStore } from '@/store/rangedChartStore';
import { RangeSelector } from '@/components/ui/RangeSelector';
import { ChartContainer } from '@/components/ChartContainer';
import { Spinner } from '@/components/ui/spinner';
import { Error } from '@/components/ui/error';

interface ChartSectionProps<TData> {
    title: string;
    description: string;
    /** The chart's own store, from `createRangedChartStore`. */
    store: UseBoundStore<StoreApi<RangedChartStore<TData>>>;
    /** Whether a loaded range turned out to have nothing to draw. */
    isEmpty: (data: TData) => boolean;
    height?: string;
    children: (data: TData) => ReactNode;
}

/**
 * A titled chart with its own time-range picker.
 *
 * Owns everything around the drawing — the heading, the picker, and the wait for
 * data — so each chart component is left with nothing but turning numbers into
 * marks. The chart is a render prop rather than an element so it only ever sees
 * data that has actually arrived, and sees it fully typed.
 */
export function ChartSection<TData>({
    title,
    description,
    store,
    isEmpty,
    height,
    children,
}: ChartSectionProps<TData>) {
    const { ranges, range, data, dataRange, loadingState } = store(
        useShallow((state) => ({
            ranges: state.ranges,
            range: state.range,
            data: state.data,
            dataRange: state.dataRange,
            loadingState: state.loadingState,
        }))
    );

    // The store is module-level and never changes identity, so this runs once.
    useEffect(() => {
        store.getState().load();
    }, [store]);

    const loading = loadingState === 'Loading';

    return (
        <section>
            <header className="mb-3 flex flex-wrap items-start justify-between gap-3">
                <div>
                    <h2 className="text-lg font-semibold text-slate-100">{title}</h2>
                    <p className="text-sm text-slate-400">{description}</p>
                </div>

                <RangeSelector
                    ranges={ranges}
                    selectedKey={range.key}
                    onSelect={(key) => store.getState().selectRange(key)}
                    label={`${title} time range`}
                />
            </header>

            {/* Once a range has been drawn it stays on screen, dimmed, while the next
                one loads — the panel never collapses to a spinner and jumps back. */}
            {data === null ? (
                loadingState === 'Error' ? <Error /> : <Spinner />
            ) : (
                <div className={`transition-opacity ${loading ? 'opacity-40' : 'opacity-100'}`}>
                    {isEmpty(data) ? (
                        <p className="py-12 text-center text-sm text-slate-400">
                            Nothing was recorded in this period.
                        </p>
                    ) : (
                        // Keyed by the range the data belongs to, so a new scale mounts
                        // a new chart. Nivo animates its axis ticks between renders and
                        // leaves the outgoing ones behind, which strands labels from the
                        // previous range — and its y-axis — on top of this one. Keying
                        // on the *selected* range instead would remount a beat too early,
                        // while the old numbers are still on screen, and change nothing.
                        // Tooltips are drawn above the point they belong to, so one
                        // read off the top of a rising line lands outside the frame.
                        // The chart may overflow so the tooltip stays whole.
                        <ChartContainer
                            key={dataRange ?? undefined}
                            height={height}
                            className="bg-none"
                            overflowVisible
                        >
                            {children(data)}
                        </ChartContainer>
                    )}
                </div>
            )}

            {loadingState === 'Error' && data !== null && (
                <p className="mt-2 text-center text-sm text-red-400">Could not load this range.</p>
            )}
        </section>
    );
}
