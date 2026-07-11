// Payload shape for GET/POST /api/import-export/{export,import}.
//
// IMPORTANT: ExportToursUseCase/ImportToursUseCase serialize manually with
// `new JsonSerializerOptions(JsonSerializerDefaults.Web)` and do NOT add the global
// JsonStringEnumConverter that Program.cs registers for the rest of the API. So — unlike
// every other endpoint — transportType/difficulty here are the raw numeric C# enum values,
// not strings. Do not reuse the app's string-based TransportType/Difficulty types for this.

// TourPlanner.Domain.Enums.TransportType: Walking=0, Hiking=1, Bicycling=2, Car=3,
// PublicTransport=4, Train=5, Bus=6, Mixed=7
// TourPlanner.Domain.TourDifficulty: VeryEasy=1, Easy=2, Medium=3, Hard=4, Extreme=5

export interface ExportedTourLog {
  id: string;
  tourId: string;
  accomplishedAt: string;
  comment: string;
  difficulty: number;
  totalDistanceKm: number;
  totalTimeMinutes: number;
  rating: number;
}

// Mirrors the array-of-TourDetailResponseDto shape written/read by Export/ImportToursUseCase.
export interface ExportedTour {
  id: string;
  name: string;
  description: string;
  from: string;
  to: string;
  transportType: number;
  distanceKm: number;
  estimatedMinutes: number;
  routeInformation: string;
  popularity: number;
  childFriendliness: number;
  imagePath: string | null;
  logs: ExportedTourLog[];
  routeGeoJson: string | null;
}

// Mirrors TourPlanner.Application.UseCases.Tours.ImportTours.ImportToursResponse
export interface ImportToursResponse {
  importedTours: number;
  importedTourLogs: number;
}
