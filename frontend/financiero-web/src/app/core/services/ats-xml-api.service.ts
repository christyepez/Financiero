import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ReadinessResponse } from './api.models';

@Injectable({ providedIn: 'root' })
export class AtsXmlApiService {
  private readonly api = inject(ApiService);

  getReadiness(period: string): Observable<ReadinessResponse> {
    return this.api.get<ReadinessResponse>('/api/financial/tax-reporting/ats-xml/readiness', { period });
  }
}
