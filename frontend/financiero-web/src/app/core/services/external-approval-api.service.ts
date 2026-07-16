import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ExternalApprovalGate, ReadinessResponse } from './api.models';

@Injectable({ providedIn: 'root' })
export class ExternalApprovalApiService {
  private readonly api = inject(ApiService);

  getAll(): Observable<ExternalApprovalGate[]> {
    return this.api.get<ExternalApprovalGate[]>('/api/financial/external-approvals');
  }

  getReadiness(scope: string): Observable<ReadinessResponse> {
    return this.api.get<ReadinessResponse>(`/api/financial/external-approvals/${scope}/readiness`);
  }
}
