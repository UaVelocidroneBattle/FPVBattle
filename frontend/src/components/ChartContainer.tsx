import { Suspense } from 'react';
import { Spinner } from "@/components/ui/spinner.tsx";

interface ChartContainerProps {
  children: React.ReactNode;
  height?: string;
  className?: string;
  overflowVisible?: boolean;
}

export function ChartContainer({
  children,
  height = "600px",
  className = "bg-slate-200",
  overflowVisible = false
}: ChartContainerProps) {

  return (
    <div
      className={`${className} w-full min-w-0 ${overflowVisible ? 'overflow-visible' : 'overflow-hidden'}`}
      style={{ height }}
    >
      <Suspense fallback={<Spinner/>}>
        {children}
      </Suspense>
    </div>
  );
}