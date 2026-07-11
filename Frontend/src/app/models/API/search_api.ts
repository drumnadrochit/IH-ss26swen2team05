import { ToursResponse } from './Tours';
import { TourLogsResponse } from './tour_log_api';

// Mirrors TourPlanner.Application.CommonDtos.Tours.TourSearchResponseDto
export interface TourSearchResponse {
  tours: ToursResponse[];
  tourLogs: TourLogsResponse[];
}
