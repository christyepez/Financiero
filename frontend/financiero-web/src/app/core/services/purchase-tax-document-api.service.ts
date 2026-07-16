import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { TaxDocumentSummary } from './api.models';

@Injectable({ providedIn: 'root' })
export class PurchaseTaxDocumentApiService {
  private readonly api = inject(ApiService);

  getByPeriod(period: string): Observable<TaxDocumentSummary[]> {
    return this.api.get<TaxDocumentSummary[]>('/api/financial/purchases', { period });
  }
}
