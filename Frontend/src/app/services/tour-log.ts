import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { TourLog } from '../models/tour_log';
import {
  TourLogsResponse,
  CreateTourLogRequest,
  UpdateTourLogRequest,
} from '../models/API/tour_log_api';

@Injectable({
  providedIn: 'root',
})
export class TourLogService {
  private http = inject(HttpClient);

  // Zwei Basispfade: Logs hängen unter einer Tour (list/create),
  // einzelne Logs haben aber ihre eigene Route (update/delete).
  private toursUrl = `${environment.apiUrl}/api/tours`;
  private logsUrl = `${environment.apiUrl}/api/tour-logs`;

  private tourLogs = signal<TourLog[]>([]);
  allLogs = this.tourLogs.asReadonly();

  /** Alle Logs einer Tour vom Backend laden. */
  loadLogsForTour(tourId: string): void {
    this.http.get<TourLogsResponse[]>(`${this.toursUrl}/${tourId}/logs`).subscribe({
      next: (data) => {
        const mapped = data.map(mapLog);
        // Logs dieser Tour ersetzen, Logs anderer Touren behalten
        this.tourLogs.update((list) => [
          ...list.filter((l) => l.tourId !== tourId),
          ...mapped,
        ]);
      },
      error: (err) => console.error('Failed to load logs', err),
    });
  }

  /** Einzelnes Log vom Backend holen. */
  getLogById(logId: string): void {
    this.http.get<TourLogsResponse>(`${this.logsUrl}/${logId}`).subscribe({
      next: (data) => {
        const mapped = mapLog(data);
        this.tourLogs.update((list) => {
          const exists = list.some((l) => l.id === mapped.id);
          return exists
            ? list.map((l) => (l.id === mapped.id ? mapped : l))
            : [...list, mapped];
        });
      },
      error: (err) => console.error('Failed to load log', err),
    });
  }

  /** Log unter einer Tour erstellen. Gibt das erstellte Log zurück, damit Aufrufer (z.B. das
   *  Dashboard) danach die zugehörige Tour neu laden können (popularity/childFriendliness ändern sich). */
  createLog(req: CreateTourLogRequest): Observable<TourLog> {
    return this.http.post<TourLogsResponse>(`${this.toursUrl}/${req.tourId}/logs`, req).pipe(
      map(mapLog),
      tap((created) => this.tourLogs.update((list) => [...list, created])),
    );
  }

  /** Log aktualisieren (eigener Basispfad). */
  updateLog(logId: string, req: UpdateTourLogRequest): Observable<TourLog> {
    return this.http.put<TourLogsResponse>(`${this.logsUrl}/${logId}`, req).pipe(
      map(mapLog),
      tap((updated) =>
        this.tourLogs.update((list) =>
          list.map((l) => (l.id === updated.id ? updated : l)),
        ),
      ),
    );
  }

  /** Log löschen (eigener Basispfad). */
  deleteLog(logId: string): Observable<void> {
    return this.http.delete<void>(`${this.logsUrl}/${logId}`).pipe(
      tap(() => this.tourLogs.update((list) => list.filter((l) => l.id !== logId))),
    );
  }

  /** Logs einer Tour aus dem lokalen State lesen (synchron, fürs Template). */
  getLogsForTour(tourId: string): TourLog[] {
    return this.tourLogs().filter((l) => l.tourId === tourId);
  }
}

/** API-Response → internes TourLog-Model. */
export function mapLog(r: TourLogsResponse): TourLog {
  return {
    id: r.id,
    tourId: r.tourId,
    dateTime: r.accomplishedAt,
    comment: r.comment,
    difficulty: r.difficulty,
    totalDistance: r.totalDistanceKm,
    totalTime: r.totalTimeMinutes,
    rating: r.rating,
  };
}
