import { Difficulty } from '../tour_log';

// Mirrors TourPlanner.Application.CommonDtos.TourLogs.TourLogResponseDto
export interface TourLogsResponse {
  id: string;
  tourId: string;
  accomplishedAt: string;
  comment: string;
  difficulty: Difficulty;
  totalDistanceKm: number;
  totalTimeMinutes: number;
  rating: number;
}

// Mirrors TourPlanner.Application.UseCases.TourLogs.CreateTourLog.CreateTourLogRequest
// tourId is also supplied via the route (POST /api/tours/{tourId}/logs); the controller
// overwrites it from there, but the body shape still mirrors the backend record.
export interface CreateTourLogRequest {
  tourId: string;
  accomplishedAt: string;
  comment: string;
  difficulty: Difficulty;
  totalDistanceKm: number;
  totalTimeMinutes: number;
  rating: number;
}

// Mirrors TourPlanner.Application.UseCases.TourLogs.UpdateTourLog.UpdateTourLogRequest
// tourLogId is also supplied via the route (PUT /api/tour-logs/{tourLogId}).
export interface UpdateTourLogRequest {
  tourLogId: string;
  accomplishedAt: string;
  comment: string;
  difficulty: Difficulty;
  totalDistanceKm: number;
  totalTimeMinutes: number;
  rating: number;
}
