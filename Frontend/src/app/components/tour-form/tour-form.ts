import {
  Component, Input, Output, EventEmitter,
  signal, OnInit, ChangeDetectionStrategy
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Tour, TransportType } from '../../models/tour';
import { CreateTourRequest, UpdateTourRequest } from '../../models/API/Tours';
import { OpenRouteService } from '../../services/open-route';
import { formatTime } from '../../utils/format';

@Component({
  selector: 'app-tour-form',
  standalone: true,
  imports: [FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './tour-form.html',
  styleUrls: ['./tour-form.scss']
})
export class TourFormComponent implements OnInit {

  @Input() tour: Tour | null = null;

  @Output() tourSaved = new EventEmitter<CreateTourRequest | UpdateTourRequest>();
  @Output() cancel = new EventEmitter<void>();
  // The dashboard owns the single shared map-display; this is how the form's live
  // geocoding/route preview (from typing a location, not just clicking the map) reaches it.
  @Output() previewChanged = new EventEmitter<{
    from: [number, number] | null;
    to: [number, number] | null;
    routeGeoJson: any;
  }>();

  isEditMode = signal(false);
  isLoading = signal(false);

  name = signal('');
  description = signal('');
  from = signal('');
  to = signal('');
  transportType = signal<TransportType>(TransportType.Hiking);

  fromCoords = signal<[number, number] | null>(null);
  toCoords = signal<[number, number] | null>(null);

  distance = signal<number>(0);
  estimatedTime = signal<number>(0);
  routeGeoJson = signal<any>(null);

  errors = signal<Record<string, string>>({});
  statusMessage = signal('');

  transportTypes: { value: TransportType; label: string }[] = [
    { value: TransportType.Walking, label: 'Walking' },
    { value: TransportType.Hiking, label: 'Hiking' },
    { value: TransportType.Bicycling, label: 'Bicycling' },
    { value: TransportType.Car, label: 'Car' },
    { value: TransportType.PublicTransport, label: 'Public Transport' },
    { value: TransportType.Train, label: 'Train' },
    { value: TransportType.Bus, label: 'Bus' },
    { value: TransportType.Mixed, label: 'Mixed' },
  ];

  constructor(private ors: OpenRouteService) {}

  ngOnInit(): void {
    if (this.tour) {
      this.isEditMode.set(true);
      this.name.set(this.tour.name);
      this.description.set(this.tour.description);
      this.from.set(this.tour.from);
      this.to.set(this.tour.to);
      this.transportType.set(this.tour.transportType);
      this.fromCoords.set(this.tour.fromCoords);
      this.toCoords.set(this.tour.toCoords);
      this.distance.set(this.tour.distance);
      this.estimatedTime.set(this.tour.estimatedTime);
      this.routeGeoJson.set(this.tour.routeGeoJson);
    }
  }

  onFieldChange(field: string, event: Event): void {
    const value = (event.target as HTMLInputElement | HTMLTextAreaElement).value;
    switch (field) {
      case 'name': this.name.set(value); break;
      case 'description': this.description.set(value); break;
      case 'from': this.from.set(value); break;
      case 'to': this.to.set(value); break;
    }
    this.errors.update(e => { const c = { ...e }; delete c[field]; return c; });
  }

  async onFromBlur(): Promise<void> {
    const text = this.from().trim();
    if (!text) return;
    this.statusMessage.set('Geocoding start location...');
    const coords = await this.ors.geocode(text);
    if (coords) {
      this.fromCoords.set(coords);
      this.statusMessage.set('Start location found');
      this.emitPreview();
      this.tryCalculateRoute();
    } else {
      this.statusMessage.set('Could not find start location');
    }
  }

  async onToBlur(): Promise<void> {
    const text = this.to().trim();
    if (!text) return;
    this.statusMessage.set('Geocoding destination...');
    const coords = await this.ors.geocode(text);
    if (coords) {
      this.toCoords.set(coords);
      this.statusMessage.set('Destination found');
      this.emitPreview();
      this.tryCalculateRoute();
    } else {
      this.statusMessage.set('Could not find destination');
    }
  }

  async onMapFromSelected(coords: [number, number]): Promise<void> {
    this.fromCoords.set(coords);
    this.emitPreview();
    const name = await this.ors.reverseGeocode(coords[0], coords[1]);
    this.from.set(name ?? `${coords[0].toFixed(4)}, ${coords[1].toFixed(4)}`);
    this.tryCalculateRoute();
  }

  async onMapToSelected(coords: [number, number]): Promise<void> {
    this.toCoords.set(coords);
    this.emitPreview();
    const name = await this.ors.reverseGeocode(coords[0], coords[1]);
    this.to.set(name ?? `${coords[0].toFixed(4)}, ${coords[1].toFixed(4)}`);
    this.tryCalculateRoute();
  }

  private async tryCalculateRoute(): Promise<void> {
    const f = this.fromCoords();
    const t = this.toCoords();
    if (!f || !t) return;

    this.isLoading.set(true);
    this.statusMessage.set('Calculating route...');

    const profile = this.ors.getProfile(this.transportType());
    const result = await this.ors.getRoute(f, t, profile);

    if (result) {
      this.distance.set(result.distance);
      this.estimatedTime.set(result.duration);
      this.routeGeoJson.set(result.geoJson);
      this.statusMessage.set(`Route: ${result.distance} km, ~${this.formatTime(result.duration)}`);
    } else {
      this.statusMessage.set('Route calculation failed');
    }
    this.isLoading.set(false);
    this.emitPreview();
  }

  private emitPreview(): void {
    this.previewChanged.emit({
      from: this.fromCoords(),
      to: this.toCoords(),
      routeGeoJson: this.routeGeoJson(),
    });
  }

  onTransportChange(type: TransportType): void {
    this.transportType.set(type);
    this.tryCalculateRoute();
  }

  validate(): boolean {
    const errs: Record<string, string> = {};
    if (!this.name().trim()) errs['name'] = 'Name is required';
    if (!this.from().trim()) errs['from'] = 'Start location is required';
    if (!this.to().trim()) errs['to'] = 'Destination is required';
    this.errors.set(errs);
    return Object.keys(errs).length === 0;
  }

  onSave(): void {
    if (!this.validate()) return;

    // Distance/coords/route are computed server-side (backend calls OpenRouteService itself),
    // so only the fields the real CreateTourRequest/UpdateTourRequest accept are sent.
    const base = {
      name: this.name().trim(),
      description: this.description().trim(),
      from: this.from().trim(),
      to: this.to().trim(),
      transportType: this.transportType(),
    };

    const payload: CreateTourRequest | UpdateTourRequest = this.tour
      ? { tourId: this.tour.id, ...base }
      : base;

    this.tourSaved.emit(payload);
  }

  onCancel(): void {
    this.cancel.emit();
  }

  formatTime = formatTime;
}
