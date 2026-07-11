export interface Tour {
  id: string;                // Guid from backend
  name: string;
  description: string;
  from: string;
  to: string;
  transportType: TransportType;
  distance: number;          // km — backend: DistanceKm, computed server-side via OpenRouteService
  estimatedTime: number;     // minutes — backend: EstimatedMinutes, computed server-side
  routeGeoJson: any | null;  // client-only live-preview GeoJSON; backend never returns this
  imagePath: string | null;  // backend: ImagePath, set via the separate upload-image endpoint

  // Computed attributes
  popularity: number;            // derived from number of logs
  childFriendliness: number;     // derived from difficulty, time, distance of logs

  // Coordinates for map markers ([lat, lng]) — backend: FromCoords/ToCoords
  fromCoords: [number, number] | null;
  toCoords: [number, number] | null;
}

// Mirrors TourPlanner.Domain.Enums.TransportType exactly. The backend registers a
// JsonStringEnumConverter globally, so these values are sent/received as strings on the wire.
export enum TransportType {
  Walking = 'Walking',
  Hiking = 'Hiking',
  Bicycling = 'Bicycling',
  Car = 'Car',
  PublicTransport = 'PublicTransport',
  Train = 'Train',
  Bus = 'Bus',
  Mixed = 'Mixed',
}
