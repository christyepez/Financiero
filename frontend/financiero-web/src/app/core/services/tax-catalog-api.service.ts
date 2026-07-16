import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { CatalogItem, TaxCatalogSummary } from './api.models';

@Injectable({ providedIn: 'root' })
export class TaxCatalogApiService {
  private readonly api = inject(ApiService);

  getAll(): Observable<TaxCatalogSummary> {
    return this.api.get<TaxCatalogSummary>('/api/financial/tax-catalogs');
  }

  getPurchaseDocumentTypes(): Observable<{ version?: string; disclaimer?: string; items?: CatalogItem[] }> {
    return this.api.get('/api/financial/tax-catalogs/purchase-document-types');
  }

  getSupportDocumentTypes(): Observable<{ version?: string; disclaimer?: string; items?: CatalogItem[] }> {
    return this.api.get('/api/financial/tax-catalogs/support-document-types');
  }

  getVoidedDocumentTypes(): Observable<{ version?: string; disclaimer?: string; items?: CatalogItem[] }> {
    return this.api.get('/api/financial/tax-catalogs/voided-document-types');
  }

  getPurchaseTaxCodes(): Observable<{ version?: string; disclaimer?: string; items?: CatalogItem[] }> {
    return this.api.get('/api/financial/tax-catalogs/purchase-tax-codes');
  }

  getSupplierIdentificationTypes(): Observable<{ version?: string; disclaimer?: string; items?: CatalogItem[] }> {
    return this.api.get('/api/financial/tax-catalogs/supplier-identification-types');
  }
}
