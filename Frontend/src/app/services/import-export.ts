import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ExportedTour, ImportToursResponse } from '../models/API/import_export_api';

@Injectable({ providedIn: 'root' })
export class ImportExportService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/api/import-export`;

  /** Backend only exports the whole account (all tours + their logs), never a single tour. */
  exportAll(): Observable<ExportedTour[]> {
    return this.http.get<ExportedTour[]>(`${this.baseUrl}/export`);
  }

  importFile(file: File): Observable<ImportToursResponse> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ImportToursResponse>(`${this.baseUrl}/import`, formData);
  }
}
