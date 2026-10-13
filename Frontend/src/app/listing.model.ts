export interface Listing {
  id: string; source: string; address: string; city: string; state: string; zip: string;
  price: number; bedrooms: number; bathrooms: number; sqft: number; latitude: number;
  longitude: number; listedDate: string; status: string; description: string;
}
export interface ListingGroup {
  groupId: string; preferred: Listing; records: Listing[];
  hasConflictingZip: boolean; hasConflictingPrice: boolean;
}
export interface Stats {
  totalRecords: number; uniqueProperties: number; matchedGroups: number;
  conflictingZipGroups: number; activeProperties: number;
}
