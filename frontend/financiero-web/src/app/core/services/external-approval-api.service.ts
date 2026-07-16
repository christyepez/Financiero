import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { AddExternalApprovalEvidenceReferenceRequest, CreateExternalApprovalRequest, ExternalApprovalGate, ExternalApprovalRequestSummary, ReadinessResponse, RecordExternalApprovalDecisionRequest } from './api.models';

@Injectable({ providedIn: 'root' })
export class ExternalApprovalApiService {
  private readonly api = inject(ApiService);

  getAll(): Observable<ExternalApprovalGate[]> {
    return this.api.get<ExternalApprovalGate[]>('/api/financial/external-approvals');
  }

  getReadiness(scope: string): Observable<ReadinessResponse> {
    return this.api.get<ReadinessResponse>('/api/financial/external-approvals/readiness', { scope });
  }

  listRequests(): Observable<ExternalApprovalRequestSummary[]> {
    return this.api.get<ExternalApprovalRequestSummary[]>('/api/financial/external-approval-requests');
  }

  createRequest(request: CreateExternalApprovalRequest): Observable<ExternalApprovalRequestSummary> {
    return this.api.post<ExternalApprovalRequestSummary>('/api/financial/external-approval-requests', request);
  }

  submit(id: string): Observable<ExternalApprovalRequestSummary> {
    return this.api.post<ExternalApprovalRequestSummary>(`/api/financial/external-approval-requests/${id}/submit`, {});
  }

  startReview(id: string): Observable<ExternalApprovalRequestSummary> {
    return this.api.post<ExternalApprovalRequestSummary>(`/api/financial/external-approval-requests/${id}/start-review`, {});
  }

  addEvidenceReference(id: string, request: AddExternalApprovalEvidenceReferenceRequest): Observable<ExternalApprovalRequestSummary> {
    return this.api.post<ExternalApprovalRequestSummary>(`/api/financial/external-approval-requests/${id}/evidence-references`, request);
  }

  recordDecision(id: string, request: RecordExternalApprovalDecisionRequest): Observable<ExternalApprovalRequestSummary> {
    return this.api.post<ExternalApprovalRequestSummary>(`/api/financial/external-approval-requests/${id}/decision`, request);
  }

  cancel(id: string, reason: string): Observable<ExternalApprovalRequestSummary> {
    return this.api.post<ExternalApprovalRequestSummary>(`/api/financial/external-approval-requests/${id}/cancel`, { reason, actorDisplayName: 'Usuario Portal foundation' });
  }
}
