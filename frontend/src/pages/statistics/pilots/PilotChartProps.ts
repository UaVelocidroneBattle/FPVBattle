import { PilotResult } from "@/api/client";

export default interface PilotsChartProps {
  pilots: (string | null)[];
  results: PilotResult[][];
}
