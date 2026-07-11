import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { Tour } from '../models/tour';
import {
  ToursResponse,
  TourDetailsResponse,
  CreateTourRequest,
  UpdateTourRequest,
  TourInsightsResponse,
  UploadTourImageResponse,
} from '../models/API/Tours';

@Injectable({ providedIn: 'root' })
export class TourService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/api/tours`;

  private tours = signal<Tour[]>([]);
  allTours = this.tours.asReadonly();

  // ── Read ──

  loadTours(): void {
    this.http.get<ToursResponse[]>(this.baseUrl).subscribe({
      next: (data) => this.tours.set(data.map(mapTour)),
      error: (err) => console.error('Failed to load tours', err),
    });
  }

  // Returns the merged Tour so callers (e.g. after a log change) can refresh their
  // selection. The detail endpoint doesn't return coordinates, so those are preserved
  // from whatever is already cached rather than being nulled out.
  loadTourById(tourId: string): Observable<Tour> {
    return this.http.get<TourDetailsResponse>(`${this.baseUrl}/${tourId}`).pipe(
      map(mapTourDetail),
      map((detail) => {
        const existing = this.tours().find((t) => t.id === detail.id);
        return existing
          ? { ...detail, fromCoords: existing.fromCoords, toCoords: existing.toCoords }
          : detail;
      }),
      tap((merged) =>
        this.tours.update((list) => {
          const exists = list.some((t) => t.id === merged.id);
          return exists
            ? list.map((t) => (t.id === merged.id ? merged : t))
            : [...list, merged];
        }),
      ),
    );
  }

  loadRecommendations(): void {
    this.http.get<ToursResponse[]>(`${this.baseUrl}/recommendations`).subscribe({
      next: (data) => console.log('recommendations', data.map(mapTour)),
      error: (err) => console.error('Failed to load recommendations', err),
    });
  }

  // ── Write ──
  // Return the mapped Tour so callers (e.g. the dashboard) can select the
  // server-computed result (route/distance/coords are computed backend-side).

  createTour(req: CreateTourRequest): Observable<Tour> {
    return this.http.post<ToursResponse>(this.baseUrl, req).pipe(
      map(mapTour),
      tap((created) => this.tours.update((list) => [...list, created])),
    );
  }

  updateTour(req: UpdateTourRequest): Observable<Tour> {
    return this.http.put<ToursResponse>(`${this.baseUrl}/${req.tourId}`, req).pipe(
      map(mapTour),
      tap((updated) =>
        this.tours.update((list) =>
          list.map((t) => (t.id === updated.id ? updated : t)),
        ),
      ),
    );
  }

  deleteTour(tourId: string): void {
    this.http.delete(`${this.baseUrl}/${tourId}`).subscribe({
      next: () =>
        this.tours.update((list) => list.filter((t) => t.id !== tourId)),
      error: (err) => console.error('Failed to delete tour', err),
    });
  }

  // ── Insights ──

  loadInsights(tourId: string): void {
    this.http
      .get<TourInsightsResponse>(`${this.baseUrl}/${tourId}/insights`)
      .subscribe({
        next: (ins) =>
          this.tours.update((list) =>
            list.map((t) =>
              t.id === tourId
                ? { ...t, popularity: ins.popularity, childFriendliness: ins.childFriendliness }
                : t,
            ),
          ),
        error: (err) => console.error('Failed to load insights', err),
      });
  }

  // ── Image ──

  // Returns the mapped Tour so callers can refresh the displayed image afterwards
  // (the stored path can stay identical if the file was re-uploaded under the same name).
  uploadImage(tourId: string, file: File): Observable<Tour> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<UploadTourImageResponse>(`${this.baseUrl}/${tourId}/image`, formData).pipe(
      map((result) => {
        const existing = this.tours().find((t) => t.id === tourId);
        return { ...(existing as Tour), imagePath: result.imagePath };
      }),
      tap((updated) =>
        this.tours.update((list) => list.map((t) => (t.id === tourId ? updated : t))),
      ),
    );
  }

  // The image endpoint requires the JWT (like everything else), so a plain <img src="..."> can't
  // load it directly — fetch it via HttpClient (which attaches the token) and hand back a blob URL.
  getImageBlobUrl(tourId: string): Observable<string> {
    return this.http
      .get(`${this.baseUrl}/${tourId}/image`, { responseType: 'blob' })
      .pipe(map((blob) => URL.createObjectURL(blob)));
  }
}

/** Koordinaten-Array → [lat,lng]-Tupel (null bei fehlend/[0,0]). Backend liefert bereits [lat,lng]. */
function toCoords(arr: number[] | null | undefined): [number, number] | null {
  if (!arr || arr.length < 2) return null;
  if (arr[0] === 0 && arr[1] === 0) return null;  // [0,0] = Geocoding fehlgeschlagen
  return [arr[0], arr[1]];
}

/** Backend liefert RouteGeoJson als JSON-String; Leaflet (L.geoJSON) braucht das geparste Objekt. */
function parseGeoJson(raw: string | null): any | null {
  if (!raw) return null;
  try {
    return JSON.parse(raw);
  } catch {
    return null;
  }
}

// Real ORS-computed values come back with long floating-point tails (e.g. 213.75 minutes,
// 78.3322 km) — the fallback estimator already rounds, but the success path doesn't.
function round1(n: number): number {
  return Math.round(n * 10) / 10;
}

/** Listen-Response → internes Tour-Model. */
export function mapTour(r: ToursResponse): Tour {
  return {
    id: r.id,
    name: r.name,
    description: r.description,
    from: r.from,
    to: r.to,
    transportType: r.transportType,
    distance: round1(r.distanceKm),
    estimatedTime: Math.round(r.estimatedMinutes),
    popularity: r.popularity,
    childFriendliness: round1(r.childFriendliness),
    imagePath: r.imagePath,
    fromCoords: toCoords(r.fromCoords),
    toCoords: toCoords(r.toCoords),
    routeGeoJson: parseGeoJson(r.routeGeoJson),
  };
}

/** Detail-Response → internes Tour-Model. Backend liefert hier keine Koordinaten. */
function mapTourDetail(r: TourDetailsResponse): Tour {
  return {
    id: r.id,
    name: r.name,
    description: r.description,
    from: r.from,
    to: r.to,
    transportType: r.transportType,
    distance: round1(r.distanceKm),
    estimatedTime: Math.round(r.estimatedMinutes),
    popularity: r.popularity,
    childFriendliness: round1(r.childFriendliness),
    imagePath: r.imagePath,
    fromCoords: null,
    toCoords: null,
    routeGeoJson: parseGeoJson(r.routeGeoJson),
  };
}
