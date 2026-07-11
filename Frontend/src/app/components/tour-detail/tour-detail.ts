import {
  Component, Input, Output, EventEmitter, OnDestroy,
  signal, computed, ChangeDetectionStrategy
} from '@angular/core';
import { Tour, TransportType } from '../../models/tour';
import { TourLog, difficultyWeight } from '../../models/tour_log';
import { PopupComponent } from '../shared/popup/popup';
import { formatTime } from '../../utils/format';
import { TourService } from '../../services/tour';

@Component({
  selector: 'app-tour-detail',
  standalone: true,
  imports: [PopupComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './tour-detail.html',
  styleUrls: ['./tour-detail.scss']
})
export class TourDetailComponent implements OnDestroy {

  constructor(private tourService: TourService) {}

  @Input() set tour(value: Tour | null) {
    this._tour.set(value);
    this.loadImage(value);
  }

  @Input() set logs(value: TourLog[]) {
    this._logs.set(value);
  }

  @Output() editTour = new EventEmitter<Tour>();
  @Output() deleteTour = new EventEmitter<Tour>();
  @Output() addLog = new EventEmitter<Tour>();
  @Output() editLog = new EventEmitter<TourLog>();
  @Output() deleteLog = new EventEmitter<TourLog>();
  @Output() exportTour = new EventEmitter<Tour>();

  _tour = signal<Tour | null>(null);
  _logs = signal<TourLog[]>([]);
  imageBlobUrl = signal<string | null>(null);
  imageUploadError = signal('');

  activeTab = signal<'info' | 'logs'>('info');

  showDeleteLogConfirm = signal(false);
  logToDelete = signal<TourLog | null>(null);

  avgRating = computed(() => {
    const logs = this._logs();
    if (logs.length === 0) return 0;
    return +(logs.reduce((sum, l) => sum + l.rating, 0) / logs.length).toFixed(1);
  });

  totalLogs = computed(() => this._logs().length);

  avgDifficulty = computed(() => {
    const logs = this._logs();
    if (logs.length === 0) return '—';
    const avg = logs.reduce((sum, l) => sum + difficultyWeight(l.difficulty), 0) / logs.length;
    if (avg <= 1.5) return 'Very Easy';
    if (avg <= 2.5) return 'Easy';
    if (avg <= 3.5) return 'Medium';
    if (avg <= 4.5) return 'Hard';
    return 'Extreme';
  });

  deleteLogMessage = computed(() => {
    const log = this.logToDelete();
    if (!log) return '';
    return `Are you sure you want to delete this log from ${this.formatDate(log.dateTime)}?`;
  });

  getTransportLabel(type: TransportType): string {
    switch (type) {
      case TransportType.Walking: return 'Walking';
      case TransportType.Hiking: return 'Hiking';
      case TransportType.Bicycling: return 'Bicycling';
      case TransportType.Car: return 'Car';
      case TransportType.PublicTransport: return 'Public Transport';
      case TransportType.Train: return 'Train';
      case TransportType.Bus: return 'Bus';
      case TransportType.Mixed: return 'Mixed';
    }
  }

  formatTime = formatTime;

  formatDate(dateStr: string): string {
    const d = new Date(dateStr);
    return d.toLocaleDateString('de-AT', { day: '2-digit', month: 'short', year: 'numeric' });
  }

  onEdit(): void {
    const t = this._tour();
    if (t) this.editTour.emit(t);
  }

  onDelete(): void {
    const t = this._tour();
    if (t) this.deleteTour.emit(t);
  }

  onExport(): void {
    const t = this._tour();
    if (t) this.exportTour.emit(t);
  }

  onAddLog(): void {
    const t = this._tour();
    if (t) this.addLog.emit(t);
  }

  onEditLog(log: TourLog): void {
    this.editLog.emit(log);
  }

  onDeleteLogClick(event: Event, log: TourLog): void {
    event.stopPropagation();
    this.logToDelete.set(log);
    this.showDeleteLogConfirm.set(true);
  }

  onConfirmDeleteLog(): void {
    const log = this.logToDelete();
    if (log) this.deleteLog.emit(log);
    this.showDeleteLogConfirm.set(false);
    this.logToDelete.set(null);
  }

  onCancelDeleteLog(): void {
    this.showDeleteLogConfirm.set(false);
    this.logToDelete.set(null);
  }

  onImageFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = ''; // allow re-selecting the same file later
    const tour = this._tour();
    if (!file || !tour) return;

    this.imageUploadError.set('');
    this.tourService.uploadImage(tour.id, file).subscribe({
      next: (updated) => {
        this._tour.set(updated);
        this.loadImage(updated);
      },
      error: () => this.imageUploadError.set('Image upload failed. Please try a .jpg or .png file.'),
    });
  }

  private loadImage(tour: Tour | null): void {
    const previous = this.imageBlobUrl();
    if (previous) URL.revokeObjectURL(previous);
    this.imageBlobUrl.set(null);

    if (!tour?.imagePath) return;
    this.tourService.getImageBlobUrl(tour.id).subscribe({
      next: (url) => this.imageBlobUrl.set(url),
      error: () => this.imageBlobUrl.set(null),
    });
  }

  ngOnDestroy(): void {
    const current = this.imageBlobUrl();
    if (current) URL.revokeObjectURL(current);
  }
}
