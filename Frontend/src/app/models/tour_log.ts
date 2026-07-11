// Mirrors TourPlanner.Domain.TourDifficulty exactly. The backend registers a
// JsonStringEnumConverter globally (see Program.cs), so — just like TransportType —
// this is sent/received as a string on the wire, not the underlying int (VeryEasy=1 … Extreme=5).
export enum Difficulty {
  VeryEasy = 'VeryEasy',
  Easy = 'Easy',
  Medium = 'Medium',
  Hard = 'Hard',
  Extreme = 'Extreme',
}

export interface TourLog {
  id: string;
  tourId: string;
  dateTime: string;        // aus API: accomplishedAt
  comment: string;
  difficulty: Difficulty;
  totalDistance: number;   // aus API: totalDistanceKm
  totalTime: number;       // aus API: totalTimeMinutes
  rating: number;
}

/** Für die Anzeige im UI (lesbare Labels). */
export function difficultyLabel(d: Difficulty): string {
  switch (d) {
    case Difficulty.VeryEasy: return 'Very Easy';
    case Difficulty.Easy:     return 'Easy';
    case Difficulty.Medium:   return 'Medium';
    case Difficulty.Hard:     return 'Hard';
    case Difficulty.Extreme:  return 'Extreme';
  }
}

/** Numerisches Gewicht für Durchschnittsberechnungen (entspricht der Backend-Reihenfolge 1..5). */
export function difficultyWeight(d: Difficulty): number {
  switch (d) {
    case Difficulty.VeryEasy: return 1;
    case Difficulty.Easy:     return 2;
    case Difficulty.Medium:   return 3;
    case Difficulty.Hard:     return 4;
    case Difficulty.Extreme:  return 5;
  }
}
