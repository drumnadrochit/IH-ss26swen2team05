import {
  Component, AfterViewInit, OnDestroy, OnChanges, SimpleChanges,
  Input, Output, EventEmitter, ElementRef, ViewChild,
  signal, ChangeDetectionStrategy
} from '@angular/core';
import * as L from 'leaflet';

const iconDefault = L.icon({
  iconRetinaUrl: 'assets/marker-icon-2x.png',
  iconUrl: 'assets/marker-icon.png',
  shadowUrl: 'assets/marker-shadow.png',
  iconSize: [25, 41], iconAnchor: [12, 41],
  popupAnchor: [1, -34], shadowSize: [41, 41]
});
L.Marker.prototype.options.icon = iconDefault;

@Component({
  selector: 'app-map-display',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './map-display.html',
  styleUrls: ['./map-display.scss']
})
export class MapDisplayComponent implements AfterViewInit, OnDestroy, OnChanges {

  @ViewChild('mapContainer', { static: true }) mapRef!: ElementRef<HTMLDivElement>;

  @Input() center: [number, number] = [48.2082, 16.3738];
  @Input() zoom = 7;
  @Input() from: [number, number] | null = null;
  @Input() to: [number, number] | null = null;
  @Input() routeGeoJson: any = null;
  @Input() interactive = false;
  @Input() height = '100%';

  @Output() fromSelected = new EventEmitter<[number, number]>();
  @Output() toSelected = new EventEmitter<[number, number]>();

  // null = clicking the map does nothing (both points already set and the user hasn't
  // asked to redo one via the HUD chips). Otherwise the next click sets that marker.
  armedTarget = signal<'from' | 'to' | null>(null);

  private map!: L.Map;
  private fromMarker: L.Marker | null = null;
  private toMarker: L.Marker | null = null;
  private routeLayer: L.GeoJSON | null = null;
  private glowLayer: L.GeoJSON | null = null;
  private dashedLine: L.Polyline | null = null;
  private clickHandlerRegistered = false;

  private fromIcon = L.divIcon({
    className: 'custom-marker custom-marker--from',
    html: '<div class="marker-pulse"></div><div class="marker-dot"></div>',
    iconSize: [24, 24], iconAnchor: [12, 12], popupAnchor: [0, -16]
  });
  private toIcon = L.divIcon({
    className: 'custom-marker custom-marker--to',
    html: '<div class="marker-pulse"></div><div class="marker-dot"></div>',
    iconSize: [24, 24], iconAnchor: [12, 12], popupAnchor: [0, -16]
  });

  ngAfterViewInit(): void { this.initMap(); }

  ngOnChanges(changes: SimpleChanges): void {
    if (!this.map) return;
    if (changes['from'] || changes['to'] || changes['routeGeoJson']) {
      this.updateMarkers();
      this.updateRoute();
    }
    if (changes['center'] || changes['zoom']) this.map.setView(this.center, this.zoom);

    // Handle interactive toggle dynamically
    if (changes['interactive']) {
      this.setupClickHandler();
      if (this.interactive) {
        this.armFirstMissing();
      } else {
        this.armedTarget.set(null);
      }
    }
  }

  // Arms whichever point isn't set yet (the natural two-click flow for a brand new
  // tour). If both are already set, arms nothing — the user must pick a HUD chip to redo one.
  private armFirstMissing(): void {
    if (!this.from) this.armedTarget.set('from');
    else if (!this.to) this.armedTarget.set('to');
    else this.armedTarget.set(null);
  }

  /** Called when the user clicks a HUD chip to explicitly redo that point. */
  armTarget(target: 'from' | 'to'): void {
    if (!this.interactive) return;
    this.armedTarget.set(target);
  }

  ngOnDestroy(): void { this.map?.remove(); }

  private initMap(): void {
    const tiles = L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 19,
      attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    });

    this.map = L.map(this.mapRef.nativeElement, {
      center: this.center,
      zoom: this.zoom,
      layers: [tiles],
      zoomControl: false
    });

    L.control.zoom({ position: 'bottomright' }).addTo(this.map);
    setTimeout(() => this.map.invalidateSize(), 200);

    // Always register the click handler — it checks `interactive` at click time
    this.setupClickHandler();

    this.updateMarkers();
    this.updateRoute();
  }

  // Click handler is always on the map, but only acts when interactive=true
  private setupClickHandler(): void {
    if (this.clickHandlerRegistered || !this.map) return;
    this.clickHandlerRegistered = true;

    this.map.on('click', (e: L.LeafletMouseEvent) => {
      // Only respond when interactive mode is on, and only for the currently-armed point.
      if (!this.interactive) return;
      const target = this.armedTarget();
      if (!target) return;

      const latlng: [number, number] = [e.latlng.lat, e.latlng.lng];
      if (target === 'from') {
        this.setFromMarker(latlng);
        this.fromSelected.emit(latlng);
        // Auto-advance to "to" only if it's still missing — otherwise this was an
        // explicit redo of "from", so stop capturing clicks until asked again.
        this.armedTarget.set(this.to ? null : 'to');
      } else {
        this.setToMarker(latlng);
        this.toSelected.emit(latlng);
        this.armedTarget.set(this.from ? null : 'from');
      }
    });
  }

  // ── Markers ──

  private updateMarkers(): void {
    if (this.from) this.setFromMarker(this.from);
    else if (this.fromMarker) { this.map.removeLayer(this.fromMarker); this.fromMarker = null; }

    if (this.to) this.setToMarker(this.to);
    else if (this.toMarker) { this.map.removeLayer(this.toMarker); this.toMarker = null; }

    this.fitBoundsToMarkers();
  }

  private setFromMarker(latlng: [number, number]): void {
    if (this.fromMarker) this.map.removeLayer(this.fromMarker);
    this.fromMarker = L.marker(latlng, { icon: this.fromIcon })
      .addTo(this.map).bindPopup('<span>Start</span>');
  }

  private setToMarker(latlng: [number, number]): void {
    if (this.toMarker) this.map.removeLayer(this.toMarker);
    this.toMarker = L.marker(latlng, { icon: this.toIcon })
      .addTo(this.map).bindPopup('<span>Destination</span>');
  }

  private fitBoundsToMarkers(): void {
    const pts: L.LatLng[] = [];
    if (this.fromMarker) pts.push(this.fromMarker.getLatLng());
    if (this.toMarker) pts.push(this.toMarker.getLatLng());
    if (pts.length === 2) this.map.fitBounds(L.latLngBounds(pts), { padding: [50, 50] });
    else if (pts.length === 1) this.map.setView(pts[0], 13);
  }

  // ── Route + Dashed Fallback ──

  private updateRoute(): void {
    if (this.routeLayer) { this.map.removeLayer(this.routeLayer); this.routeLayer = null; }
    if (this.glowLayer) { this.map.removeLayer(this.glowLayer); this.glowLayer = null; }
    if (this.dashedLine) { this.map.removeLayer(this.dashedLine); this.dashedLine = null; }

    if (this.routeGeoJson) {
      this.routeLayer = L.geoJSON(this.routeGeoJson, {
        style: () => ({ color: '#e05252', weight: 4, opacity: 0.95 })
      }).addTo(this.map);
      this.glowLayer = L.geoJSON(this.routeGeoJson, {
        style: () => ({ color: '#e05252', weight: 14, opacity: 0.18 })
      }).addTo(this.map);

      const bounds = this.routeLayer.getBounds();
      if (bounds.isValid()) this.map.fitBounds(bounds, { padding: [50, 50] });
      return;
    }

    if (this.from && this.to) {
      this.dashedLine = L.polyline([this.from, this.to], {
        color: '#e05252', weight: 2, opacity: 0.55,
        dashArray: '8, 12', dashOffset: '0'
      }).addTo(this.map);
    }
  }

  // ── Public API ──

  invalidateSize(): void { this.map && setTimeout(() => this.map.invalidateSize(), 0); }
}
