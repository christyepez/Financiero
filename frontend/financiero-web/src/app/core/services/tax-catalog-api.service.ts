import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { CatalogItem } from './api.models';

@Injectable({ providedIn: 'root' })
export class TaxCatalogApiService {
  private readonly api = inject(ApiService);

  getAll(): Observable<CatalogItem[]> {
    return this.api.get<CatalogItem[]>('/api/financial/tax-catalogs');
  }
}
