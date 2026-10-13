import { Component, OnInit, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ListingService } from './listing.service';
import { ListingGroup, Stats } from './listing.model';

@Component({
  selector: 'app-root', standalone: true, imports: [CommonModule, FormsModule],
  templateUrl: './app.component.html', styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  private listings = inject(ListingService);
  groups: ListingGroup[] = [];
  stats?: Stats;
  loading = false;
  error = '';
  priceError = '';
  get invalidPriceRange(): boolean {
    return this.minPrice !== null && this.minPrice !== undefined && this.minPrice !== ('' as unknown as number) &&
      this.maxPrice !== null && this.maxPrice !== undefined && this.maxPrice !== ('' as unknown as number) &&
      Number(this.minPrice) > Number(this.maxPrice);
  }
  validatePrices(): boolean {
    this.priceError = this.invalidPriceRange
      ? 'Minimum price cannot be greater than maximum price.' : '';
    return !this.invalidPriceRange;
  }
  applyFilters(): void {
    if (!this.validatePrices()) return;
    this.refresh();
  }
  search = '';
  city = '';
  status = '';
  minBedrooms?: number;
  minPrice?: number;
  maxPrice?: number;
  duplicatesOnly = false;
  expandedGroup = '';
  readonly cities = ['Springfield', 'Fairfax', 'Reston', 'Vienna', 'Manassas', 'Chantilly'];
  ngOnInit() { this.refresh(); }
  refresh() {
    if (!this.validatePrices()) return;
    this.loading = true; this.error = '';
    this.listings.getProperties({search:this.search, city:this.city, status:this.status,
      minBedrooms:this.minBedrooms, minPrice:this.minPrice, maxPrice:this.maxPrice,
      duplicatesOnly:this.duplicatesOnly}).subscribe({
      next: data => { this.groups = data; this.loading = false; },
      error: (err: HttpErrorResponse) => {
        this.error = err.status === 0
          ? 'Unable to reach the API. Verify that the .NET backend is running at http://localhost:5088.'
          : `Unable to load properties (HTTP ${err.status}). Check the API console and GET /api/properties in Swagger.`;
        this.loading = false;
      }
    });
    this.listings.getStats().subscribe({ next: stats => this.stats = stats, error: () => {} });
  }
  reset() {
    this.search=''; this.city=''; this.status=''; this.minBedrooms=undefined;
    this.minPrice=undefined; this.maxPrice=undefined; this.priceError=''; this.duplicatesOnly=false;
    this.refresh();
  }
  toggle(id:string) { this.expandedGroup = this.expandedGroup === id ? '' : id; }
  trackGroup(_index:number, g:ListingGroup) { return g.groupId; }
}
