/**
 * Where a track stands in its cup's queue. Admins can date a track for a specific day, which jumps the line,
 * so the wait is only ever "about".
 */
export function describeQueuePosition(position: number): string {
    return position <= 1 ? "next in line" : `#${position} in line, about ${position} days away`;
}
