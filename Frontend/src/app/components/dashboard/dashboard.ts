import { Component, OnInit, signal, computed, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { TourListComponent } from '../tour-list/tour-list';
import { TourDetailComponent } from '../tour-detail/tour-detail';
import { TourFormComponent } from '../tour-form/tour-form';
import { TourLogFormComponent } from '../tour-log-form/tour-log-form';
import { MapDisplayComponent } from '../shared/map-display/map-display';
import { PopupComponent } from '../shared/popup/popup';
import { Tour } from '../../models/tour';
import { CreateTourRequest, UpdateTourRequest } from '../../models/API/Tours';
import { TourLog } from '../../models/tour_log';
import { CreateTourLogRequest, UpdateTourLogRequest } from '../../models/API/tour_log_api';
import { TourService } from '../../services/tour';
import { TourLogService } from '../../services/tour-log';
import { ImportExportService } from '../../services/import-export';
import { AuthService } from '../../services/auth';

type BottomPanel = 'detail' | 'tour-form' | 'log-form';

@Component({
  selector: 'app-tour-dashboard',
  standalone: true,
  imports: [TourListComponent, TourDetailComponent, TourFormComponent, TourLogFormComponent, MapDisplayComponent, PopupComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.scss']
})
export class DashboardComponent implements OnInit {

  @ViewChild(TourFormComponent) tourForm?: TourFormComponent;
  @ViewChild(MapDisplayComponent) mapDisplay?: MapDisplayComponent;

  selectedTour = signal<Tour | null>(null);
  bottomPanel = signal<BottomPanel>('detail');

  editingTour = signal<Tour | null>(null);
  editingLog = signal<TourLog | null>(null);
  logFormTourId = signal<string>('');

  formFromCoords = signal<[number, number] | null>(null);
  formToCoords = signal<[number, number] | null>(null);
  formRouteGeoJson = signal<any>(null);

  importMessage = signal('');
  importError = signal(false);

  saveErrorMessage = signal('');

  // Backend already scopes GetAllTours to the current user via the JWT, so no client-side filter needed.
  tours = computed(() => this.tourService.allTours());

  // getLogsForTour() reads the service's internal signal, so this computed still
  // recomputes correctly whenever the log store changes.
  selectedTourLogs = computed(() => {
    const tour = this.selectedTour();
    return tour ? this.tourLogService.getLogsForTour(tour.id) : [];
  });

  showDrawer = computed(() => {
    return this.selectedTour() !== null || this.bottomPanel() !== 'detail';
  });

  mapFrom = computed(() => {
    if (this.bottomPanel() === 'tour-form') return this.formFromCoords();
    return this.selectedTour()?.fromCoords ?? null;
  });

  mapTo = computed(() => {
    if (this.bottomPanel() === 'tour-form') return this.formToCoords();
    return this.selectedTour()?.toCoords ?? null;
  });

  mapRouteGeoJson = computed(() => {
    if (this.bottomPanel() === 'tour-form') return this.formRouteGeoJson();
    return this.selectedTour()?.routeGeoJson ?? null;
  });

  isMapInteractive = computed(() => this.bottomPanel() === 'tour-form');

  constructor(
    private tourService: TourService,
    private tourLogService: TourLogService,
    private importExportService: ImportExportService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.tourService.loadTours();
  }

  onTourSelected(tour: Tour): void {
    if (this.selectedTour()?.id === tour.id && this.bottomPanel() === 'detail') {
      this.selectedTour.set(null);
      return;
    }
    this.selectedTour.set(tour);
    this.bottomPanel.set('detail');
    this.tourLogService.loadLogsForTour(tour.id);
    // Opening the bottom drawer shrinks the map container — without this, Leaflet keeps
    // using its stale (taller) size and draws markers/routes below the visible area.
    this.invalidateMap();
  }

  onTourCreate(): void {
    this.editingTour.set(null);
    this.clearFormCoords();
    this.bottomPanel.set('tour-form');
    this.invalidateMap();
  }

  onTourEdit(tour: Tour): void {
    this.editingTour.set(tour);
    this.formFromCoords.set(tour.fromCoords);
    this.formToCoords.set(tour.toCoords);
    this.formRouteGeoJson.set(tour.routeGeoJson);
    this.bottomPanel.set('tour-form');
    this.invalidateMap();
  }

  onTourDelete(tour: Tour): void {
    this.tourService.deleteTour(tour.id);
    if (this.selectedTour()?.id === tour.id) {
      this.selectedTour.set(null);
    }
  }

  onMapFromSelected(coords: [number, number]): void {
    this.formFromCoords.set(coords);
    this.tourForm?.onMapFromSelected(coords);
  }

  onMapToSelected(coords: [number, number]): void {
    this.formToCoords.set(coords);
    this.tourForm?.onMapToSelected(coords);
  }

  // Keeps the shared map in sync with the form's own preview state, e.g. when the
  // user types a location (geocoded) rather than clicking the map.
  onFormPreviewChanged(preview: { from: [number, number] | null; to: [number, number] | null; routeGeoJson: any }): void {
    this.formFromCoords.set(preview.from);
    this.formToCoords.set(preview.to);
    this.formRouteGeoJson.set(preview.routeGeoJson);
  }

  onTourFormSave(payload: CreateTourRequest | UpdateTourRequest): void {
    const request$ = 'tourId' in payload
      ? this.tourService.updateTour(payload)
      : this.tourService.createTour(payload);

    request$.subscribe({
      next: (saved) => {
        this.selectedTour.set(saved);
        this.bottomPanel.set('detail');
        this.editingTour.set(null);
        this.clearFormCoords();
        this.invalidateMap();
      },
      error: (err: HttpErrorResponse) =>
        this.saveErrorMessage.set(this.extractError(err, 'Could not save the tour. Please try again.')),
    });
  }

  onTourFormCancel(): void {
    this.bottomPanel.set('detail');
    this.editingTour.set(null);
    this.clearFormCoords();
    this.invalidateMap();
  }

  onAddLog(tour: Tour): void {
    this.editingLog.set(null);
    this.logFormTourId.set(tour.id);
    this.bottomPanel.set('log-form');
  }

  onEditLog(log: TourLog): void {
    this.editingLog.set(log);
    this.logFormTourId.set(log.tourId);
    this.bottomPanel.set('log-form');
  }

  onDeleteLog(log: TourLog): void {
    this.tourLogService.deleteLog(log.id).subscribe(() => this.refreshSelectedTour(log.tourId));
  }

  onLogFormSave(payload: CreateTourLogRequest | UpdateTourLogRequest): void {
    const request$ = 'tourLogId' in payload
      ? this.tourLogService.updateLog(payload.tourLogId, payload)
      : this.tourLogService.createLog(payload);

    const tourId = this.logFormTourId();
    request$.subscribe({
      next: () => {
        if (tourId) this.refreshSelectedTour(tourId);
        this.bottomPanel.set('detail');
        this.editingLog.set(null);
      },
      error: (err: HttpErrorResponse) =>
        this.saveErrorMessage.set(this.extractError(err, 'Could not save the log. Please try again.')),
    });
  }

  onSaveErrorPopupConfirm(): void {
    this.saveErrorMessage.set('');
  }

  // Popularity/childFriendliness change whenever logs change, so reload the tour from
  // the backend rather than recomputing them client-side.
  private refreshSelectedTour(tourId: string): void {
    this.tourService.loadTourById(tourId).subscribe((tour) => {
      if (this.selectedTour()?.id === tourId) {
        this.selectedTour.set(tour);
      }
    });
  }

  onLogFormCancel(): void {
    this.bottomPanel.set('detail');
    this.editingLog.set(null);
  }

  closeDrawer(): void {
    this.selectedTour.set(null);
    this.bottomPanel.set('detail');
    this.editingTour.set(null);
    this.editingLog.set(null);
    this.clearFormCoords();
    this.invalidateMap();
  }

  // Backend only supports exporting the whole account, so fetch everything and pick out
  // just the requested tour — wrapped in an array to stay valid for re-import.
  onExportTour(tour: Tour): void {
    this.importExportService.exportAll().subscribe({
      next: (allTours) => {
        const match = allTours.find((t) => t.id === tour.id);
        if (!match) return;
        const json = JSON.stringify([match], null, 2);
        const blob = new Blob([json], { type: 'application/json' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `tour-${tour.name.replace(/\s+/g, '-').toLowerCase()}.json`;
        a.click();
        URL.revokeObjectURL(url);
      },
      error: (err) => console.error('Failed to export tour', err),
    });
  }

  onImportFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = ''; // allow re-selecting the same file again later

    if (!file) return;

    this.importExportService.importFile(file).subscribe({
      next: (result) => {
        this.tourService.loadTours();
        this.importError.set(false);
        this.importMessage.set(
          `Imported ${result.importedTours} tour(s) and ${result.importedTourLogs} log(s).`,
        );
      },
      error: (err: HttpErrorResponse) => {
        this.importError.set(true);
        this.importMessage.set(this.extractError(err, 'Import failed. Please check the file and try again.'));
      },
    });
  }

  onImportPopupConfirm(): void {
    this.importMessage.set('');
  }

  private extractError(err: HttpErrorResponse, fallback: string): string {
    const body = err?.error as { error?: string; errors?: { errorMessage: string }[] } | undefined;
    if (body?.errors?.length) return body.errors[0].errorMessage;
    if (body?.error) return body.error;
    return fallback;
  }

  onLogout(): void {
    this.authService.logout();
    this.router.navigate(['/auth']);
  }

  private clearFormCoords(): void {
    this.formFromCoords.set(null);
    this.formToCoords.set(null);
    this.formRouteGeoJson.set(null);
  }

  /** Tell Leaflet to recalculate size after layout change */
  private invalidateMap(): void {
    setTimeout(() => this.mapDisplay?.invalidateSize(), 100);
  }
}
