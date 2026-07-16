import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { AddPurchaseLineRequest, AddPurchaseTaxRequest, CreatePurchaseTaxDocumentRequest, TaxDocumentSummary } from './api.models';

@Injectable({ providedIn: 'root' })
export class PurchaseTaxDocumentApiService {
  private readonly api = inject(ApiService);

  getByPeriod(period: string): Observable<TaxDocumentSummary[]> {
    return this.api.get<TaxDocumentSummary[]>('/api/financial/purchases', { period });
  }

  createPurchase(request: CreatePurchaseTaxDocumentRequest): Observable<TaxDocumentSummary> {
    return this.api.post<TaxDocumentSummary>('/api/financial/purchases', request);
  }

  addLine(id: string, request: AddPurchaseLineRequest): Observable<TaxDocumentSummary> {
    return this.api.post<TaxDocumentSummary>(`/api/financial/purchases/${id}/lines`, request);
  }

  addTax(id: string, request: AddPurchaseTaxRequest): Observable<TaxDocumentSummary> {
    return this.api.post<TaxDocumentSummary>(`/api/financial/purchases/${id}/taxes`, request);
  }

  validatePurchase(id: string): Observable<TaxDocumentSummary> {
    return this.api.post<TaxDocumentSummary>(`/api/financial/purchases/${id}/validate`, {});
  }
}
