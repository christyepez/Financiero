import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { RegisterVoidedTaxDocumentRequest, TaxDocumentSummary } from './api.models';

@Injectable({ providedIn: 'root' })
export class VoidedTaxDocumentApiService {
  private readonly api = inject(ApiService);

  getByPeriod(period: string): Observable<TaxDocumentSummary[]> {
    return this.api.get<TaxDocumentSummary[]>('/api/financial/voided-documents', { period });
  }

  registerVoidedDocument(request: RegisterVoidedTaxDocumentRequest): Observable<TaxDocumentSummary> {
    return this.api.post<TaxDocumentSummary>('/api/financial/voided-documents', request);
  }
}
