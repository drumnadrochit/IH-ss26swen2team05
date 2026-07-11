/** "Xh Ym" / "Xh" / "Ymin". Rounds first so a fractional input (e.g. 213.75) never
 *  leaks into the remainder (e.g. "3h 33.75m"). */
export function formatTime(minutes: number): string {
  const total = Math.round(minutes);
  const h = Math.floor(total / 60);
  const m = total % 60;
  if (h === 0) return `${m}min`;
  if (m === 0) return `${h}h`;
  return `${h}h ${m}m`;
}
