import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../environments/environment';
import { Tour } from '../models/tour';
import { TourLog } from '../models/tour_log';
import { TourSearchResponse } from '../models/API/search_api';
import { mapTour } from './tour';
import { mapLog } from './tour-log';

export interface SearchResult {
  tours: Tour[];
  tourLogs: TourLog[];
}

@Injectable({ providedIn: 'root' })
export class SearchService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/api/search`;

  /** Full-text search across the current user's tours and logs (incl. computed values). */
  search(query: string): Observable<SearchResult> {
    const params = new HttpParams().set('q', query);
    return this.http.get<TourSearchResponse>(this.baseUrl, { params }).pipe(
      map((res) => ({
        tours: res.tours.map(mapTour),
        tourLogs: res.tourLogs.map(mapLog),
      })),
    );
  }
}
