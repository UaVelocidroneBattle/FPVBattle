import { Suspense } from 'react';

interface ChartContainerProps {
  children: React.ReactNode;
  height?: string;
  className?: string;
}

export function ChartContainer({
  children,
  height = "600px",
  className = "bg-slate-200 rounded-lg"
}: ChartContainerProps) {

  return (
    <div
      className={`${className} w-full overflow-hidden min-w-0`}
      style={{ height }}
    >
      <Suspense fallback={<div>Loading...</div>}>
        {children}
      </Suspense>
    </div>
  );
}