import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Listing, ListingGroup, Stats } from './listing.model';
import { environment } from '../environments/environment';
@Injectable({providedIn: 'root'})
export class ListingService {
  private http = inject(HttpClient);
  private readonly base = environment.apiBaseUrl.replace(/\/$/, '');
  getProperties(filters: Record<string, string | number | boolean | null | undefined>) {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(filters)) {
      if (value === null || value === undefined || value === '' || value === false) continue;
      if (typeof value === 'number' && !Number.isFinite(value)) continue;
      params = params.set(key, String(value));
    }
    return this.http.get<ListingGroup[]>(`${this.base}/properties`, {params});
  }
  getStats() { return this.http.get<Stats>(`${this.base}/stats`); }
  getRaw() { return this.http.get<Listing[]>(`${this.base}/listings`); }
}
