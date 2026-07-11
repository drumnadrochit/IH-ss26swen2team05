import { TransportType } from '../tour';
import { TourLogsResponse } from './tour_log_api';

// Mirrors TourPlanner.Application.CommonDtos.Tours.TourSummaryResponseDto
export interface ToursResponse {
  id: string;
  name: string;
  description: string;
  from: string;
  to: string;
  transportType: TransportType;
  distanceKm: number;
  estimatedMinutes: number;
  popularity: number;
  childFriendliness: number;
  imagePath: string | null;
  fromCoords: number[] | null;
  toCoords: number[] | null;
  routeGeoJson: string | null;
}

// Mirrors TourPlanner.Application.CommonDtos.Tours.TourDetailResponseDto
// Note: unlike ToursResponse, the backend does not include from/to coordinates here.
export interface TourDetailsResponse {
  id: string;
  name: string;
  description: string;
  from: string;
  to: string;
  transportType: TransportType;
  distanceKm: number;
  estimatedMinutes: number;
  routeInformation: string;
  popularity: number;
  childFriendliness: number;
  imagePath: string | null;
  logs: TourLogsResponse[];
  routeGeoJson: string | null;
}

// Mirrors TourPlanner.Application.UseCases.Tours.CreateTour.CreateTourRequest
// Note: no imagePath/distance/coords — those are computed server-side or set via UploadImage.
export interface CreateTourRequest {
  name: string;
  description: string;
  from: string;
  to: string;
  transportType: TransportType;
}

// Mirrors TourPlanner.Application.UseCases.Tours.UpdateTour.UpdateTourRequest
export interface UpdateTourRequest {
  tourId: string;
  name: string;
  description: string;
  from: string;
  to: string;
  transportType: TransportType;
}

// Mirrors TourPlanner.Application.CommonDtos.Tours.TourInsightResponseDto
export interface TourInsightsResponse {
  tourId: string;
  logCount: number;
  averageDifficulty: number;
  averageDistanceKm: number;
  averageTimeMinutes: number;
  popularity: number;
  childFriendliness: number;
}

// Mirrors TourPlanner.Application.CommonDtos.Tours.UploadTourImageResponseDto
export interface UploadTourImageResponse {
  imagePath: string;
}

