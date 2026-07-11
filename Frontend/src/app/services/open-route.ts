import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { TransportType } from '../models/tour';

@Injectable({
  providedIn: 'root'
})
export class OpenRouteService {

  private apiKey = environment.orsApiKey;
  private baseUrl = 'https://api.openrouteservice.org';

  /**
   * Geocode a place name to coordinates
   * Returns [lat, lng] or null if not found
   */
  async geocode(placeName: string): Promise<[number, number] | null> {
    try {
      const url = `${this.baseUrl}/geocode/search?api_key=${this.apiKey}&text=${encodeURIComponent(placeName)}&size=1`;
      const res = await fetch(url);
      const data = await res.json();

      if (data.features && data.features.length > 0) {
        const coords = data.features[0].geometry.coordinates; // [lng, lat]
        return [coords[1], coords[0]]; // convert to [lat, lng]
      }
      return null;
    } catch (err) {
      console.error('Geocoding failed:', err);
      return null;
    }
  }

  /**
   * Reverse geocode coordinates to a place name
   * Returns place name or null
   */
  async reverseGeocode(lat: number, lng: number): Promise<string | null> {
    try {
      const url = `${this.baseUrl}/geocode/reverse?api_key=${this.apiKey}&point.lat=${lat}&point.lon=${lng}&size=1`;
      const res = await fetch(url);
      const data = await res.json();

      if (data.features && data.features.length > 0) {
        return data.features[0].properties.label || data.features[0].properties.name || null;
      }
      return null;
    } catch (err) {
      console.error('Reverse geocoding failed:', err);
      return null;
    }
  }

  /**
   * Get route between two points
   * Returns { geoJson, distance (km), duration (minutes) } or null
   */
  async getRoute(
    from: [number, number],
    to: [number, number],
    profile: string = 'foot-hiking'
  ): Promise<{ geoJson: any; distance: number; duration: number } | null> {
    try {
      // ORS expects [lng, lat] not [lat, lng]
      const body = {
        coordinates: [
          [from[1], from[0]],
          [to[1], to[0]]
        ]
      };

      const url = `${this.baseUrl}/v2/directions/${profile}/geojson`;
      const res = await fetch(url, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': this.apiKey
        },
        body: JSON.stringify(body)
      });

      const data = await res.json();

      if (data.features && data.features.length > 0) {
        const feature = data.features[0];
        const summary = feature.properties.summary;

        return {
          geoJson: data,
          distance: Math.round(summary.distance / 100) / 10, // meters → km, 1 decimal
          duration: Math.round(summary.duration / 60)          // seconds → minutes
        };
      }
      return null;
    } catch (err) {
      console.error('Routing failed:', err);
      return null;
    }
  }

  /**
   * Map TransportType to ORS profile.
   * ORS has no public-transport/train/bus profile, so those fall back to driving-car.
   */
  getProfile(transportType: TransportType): string {
    switch (transportType) {
      case TransportType.Walking: return 'foot-walking';
      case TransportType.Hiking: return 'foot-hiking';
      case TransportType.Bicycling: return 'cycling-regular';
      case TransportType.Car: return 'driving-car';
      case TransportType.PublicTransport: return 'driving-car';
      case TransportType.Train: return 'driving-car';
      case TransportType.Bus: return 'driving-car';
      case TransportType.Mixed: return 'driving-car';
    }
  }
}
