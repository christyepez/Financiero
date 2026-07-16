import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ReadinessResponse } from './api.models';

@Injectable({ providedIn: 'root' })
export class TaxReportingApiService {
  private readonly api = inject(ApiService);

  getAtsReadiness(period: string): Observable<ReadinessResponse> {
    return this.api.get<ReadinessResponse>('/api/financial/tax-reporting/ats-section-readiness', { period });
  }
}
