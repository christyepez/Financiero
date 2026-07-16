import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommandGuardService } from '../../core/services/command-guard.service';
import { ExternalApprovalApiService } from '../../core/services/external-approval-api.service';
import { ExternalApprovalGate, ExternalApprovalRequestSummary, ReadinessResponse } from '../../core/services/api.models';
import { CommandDisabledBannerComponent } from '../../shared/components/command-disabled-banner.component';
import { ErrorMessageComponent } from '../../shared/components/error-message.component';
import { EmptyStateComponent } from '../../shared/components/empty-state.component';
import { FoundationDisclaimerComponent } from '../../shared/components/foundation-disclaimer.component';
import { LoadingStateComponent } from '../../shared/components/loading-state.component';
import { ReadinessCardComponent } from '../../shared/components/readiness-card.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';

@Component({
  standalone: true,
  selector: 'fin-external-approvals',
  imports: [CommandDisabledBannerComponent, EmptyStateComponent, ErrorMessageComponent, FormsModule, FoundationDisclaimerComponent, LoadingStateComponent, ReadinessCardComponent, StatusBadgeComponent],
  template: `
    <fin-foundation-disclaimer text="Aprobaciones externas son foundation. Persisten metadata/referencias sanitizadas; no almacenan archivos ni habilitan producción." />
    <div class="panel">
      <span class="badge info">Metadata persistida</span>
      <span class="badge warn">Sin upload</span>
      <span class="badge bad">No habilita SRI/ATS/RIDE/XAdES</span>
      <p class="muted">Use esta vista como checklist foundation. La aprobación legal/tributaria real requiere revisión externa fuera del repositorio.</p>
    </div>
    @if (!canCommand()) {
      <fin-command-disabled-banner [reason]="guard.disabledReason('approval')" />
    }
    <section class="panel">
      <h2>Crear request foundation</h2>
      <div class="form-grid">
        <select [(ngModel)]="form.scope" [disabled]="!canCommand()">
          <option value="ATS">ATS</option>
          <option value="RIDE">RIDE</option>
          <option value="XADES">XADES</option>
          <option value="SRI_TEST">SRI_TEST</option>
          <option value="SRI_PRODUCTION">SRI_PRODUCTION</option>
          <option value="CONTENT_FILE">CONTENT_FILE</option>
          <option value="CERTIFICATE_CUSTODY">CERTIFICATE_CUSTODY</option>
          <option value="SECURITY_GATE">SECURITY_GATE</option>
          <option value="TAX_LEGAL_REVIEW">TAX_LEGAL_REVIEW</option>
          <option value="RUNBOOK">RUNBOOK</option>
          <option value="PORTAL_SHELL">PORTAL_SHELL</option>
        </select>
        <input [(ngModel)]="form.title" [disabled]="!canCommand()" placeholder="Título foundation" />
        <input [(ngModel)]="form.requirement" [disabled]="!canCommand()" placeholder="Requisito sanitizado" />
        <button type="button" [disabled]="!canCommand()" (click)="create()">Crear request</button>
      </div>
      <p class="muted">No ingrese XML, certificados, base64, URLs con tokens, datos reales ni evidencia sensible.</p>
    </section>
    <fin-loading-state [loading]="loading()" />
    <fin-error-message [message]="error()" />
    <section class="stack">
      <fin-readiness-card title="Approval workflow readiness" [data]="readiness()" />
      @if (requests().length) {
      <div class="panel">
        <h2>Requests persistidos foundation</h2>
        <table class="table">
          <thead><tr><th>Scope</th><th>Estado</th><th>Título</th><th>Evidencias</th><th>Decisión</th><th>Acciones</th></tr></thead>
          <tbody>
            @for (item of requests(); track item.id) {
              <tr>
                <td>{{ item.scope }}</td>
                <td><fin-status-badge [value]="item.status" /></td>
                <td>{{ item.title }}</td>
                <td>{{ item.evidenceReferences?.length || 0 }} referencias metadata</td>
                <td>{{ item.decisions?.[0]?.decisionKind || 'Sin decisión' }}</td>
                <td>
                  <button type="button" [disabled]="!canCommand()" (click)="submit(item)">Submit</button>
                  <button type="button" [disabled]="!canCommand()" (click)="startReview(item)">Review</button>
                  <button type="button" [disabled]="!canEvidence()" (click)="addEvidence(item)">Add ref</button>
                  <button type="button" [disabled]="!canDecision()" (click)="approve(item)">Approve foundation</button>
                  <button type="button" [disabled]="!canDecision()" (click)="reject(item)">Reject foundation</button>
                  <button type="button" [disabled]="!canCommand()" (click)="cancel(item)">Cancel</button>
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>
      } @else if (!loading()) {
        <fin-empty-state title="Sin requests persistidos" description="Puede crear requests foundation solo con flags y permiso manage." />
      }
      @if (approvals().length) {
      <div class="panel">
        <h2>Gates externos</h2>
        <table class="table">
          <thead><tr><th>Scope</th><th>Estado</th><th>Evidencia</th><th>Riesgos</th><th>Siguiente acción</th></tr></thead>
          <tbody>
            @for (item of approvals(); track item.scope) {
              <tr>
                <td>{{ item.scope || 'all' }}</td>
                <td><fin-status-badge [value]="item.status || (item.approved ? 'Approved' : 'Pending')" /></td>
                <td>{{ item.requiresEvidence ? 'Requerida' : 'No requerida' }}</td>
                <td>{{ (item.blockingRisks || []).join('; ') || 'Sin riesgos públicos reportados.' }}</td>
                <td>{{ item.recommendedNextAction || item.message || 'Requiere revisión externa sanitizada.' }}</td>
              </tr>
            }
          </tbody>
        </table>
      </div>
      } @else if (!loading()) {
        <fin-empty-state title="Sin gates externos" description="El backend no devolvió aprobaciones externas para este entorno." />
      }
    </section>
  `
})
export class ExternalApprovalsComponent {
  private readonly api = inject(ExternalApprovalApiService);
  protected readonly guard = inject(CommandGuardService);
  protected readonly approvals = signal<ExternalApprovalGate[]>([]);
  protected readonly requests = signal<ExternalApprovalRequestSummary[]>([]);
  protected readonly readiness = signal<ReadinessResponse | null>(null);
  protected readonly error = signal<string | null>(null);
  protected readonly loading = signal(true);
  protected form = { scope: 'ATS', title: 'Solicitud foundation sintética', requirement: 'Evidencia externa sanitizada requerida' };

  constructor() {
    this.loadRequests();
    this.api.getAll().subscribe({ next: value => this.approvals.set(value), error: error => this.error.set(error.message) });
    this.api.getReadiness('all').subscribe({
      next: value => this.readiness.set(value),
      error: error => this.error.set(error.message),
      complete: () => this.loading.set(false)
    });
  }

  canCommand(): boolean { return this.guard.canRunExternalApprovalCommands(); }
  canEvidence(): boolean { return this.guard.canRunEvidenceReferenceCommands(); }
  canDecision(): boolean { return this.guard.canRunApprovalDecisionCommands(); }

  create(): void {
    if (!this.canCommand() || !this.safe(this.form.title) || !this.safe(this.form.requirement)) return this.error.set('Input inseguro o incompleto para request foundation.');
    if (!confirm('Crear solicitud foundation. No habilita producción ni almacena evidencia real. ¿Continuar?')) return;
    this.api.createRequest({
      scope: this.form.scope,
      title: this.form.title,
      createdByDisplayName: 'Usuario Portal foundation',
      requirements: [{ description: this.form.requirement, requiresEvidence: false, requiresHumanReview: true }]
    }).subscribe({ next: () => this.loadRequests(), error: error => this.error.set(error.message) });
  }
  submit(item: ExternalApprovalRequestSummary): void { this.api.submit(item.id).subscribe({ next: () => this.loadRequests(), error: error => this.error.set(error.message) }); }
  startReview(item: ExternalApprovalRequestSummary): void { this.api.startReview(item.id).subscribe({ next: () => this.loadRequests(), error: error => this.error.set(error.message) }); }
  addEvidence(item: ExternalApprovalRequestSummary): void {
    const referenceId = prompt('Referencia metadata sintética sin URL/token/base64/XML/certificado', `portal-content-file-ref-${Date.now()}`) || '';
    if (!this.safe(referenceId)) return this.error.set('Referencia de evidencia insegura rechazada.');
    this.api.addEvidenceReference(item.id, { provider: 'PortalContentFilePlaceholder', referenceId, displayName: 'Referencia metadata foundation', createdByDisplayName: 'Usuario Portal foundation' }).subscribe({ next: () => this.loadRequests(), error: error => this.error.set(error.message) });
  }
  approve(item: ExternalApprovalRequestSummary): void { this.decision(item, 'ApprovedFoundation', 'Aprobación foundation; no habilita producción.'); }
  reject(item: ExternalApprovalRequestSummary): void { this.decision(item, 'RejectedFoundation', 'Rechazo foundation con razón sanitizada.'); }
  cancel(item: ExternalApprovalRequestSummary): void { this.api.cancel(item.id, 'Cancelación foundation').subscribe({ next: () => this.loadRequests(), error: error => this.error.set(error.message) }); }
  private decision(item: ExternalApprovalRequestSummary, decision: 'ApprovedFoundation' | 'RejectedFoundation', reason: string): void {
    this.api.recordDecision(item.id, { decision, reason, decidedByDisplayName: 'Revisor Portal foundation' }).subscribe({ next: () => this.loadRequests(), error: error => this.error.set(error.message) });
  }
  private loadRequests(): void {
    this.api.listRequests().subscribe({ next: value => this.requests.set(value), error: error => this.error.set(error.message) });
  }
  private safe(value: string): boolean {
    const certificateMarker = 'BEGIN' + ' CERTIFICATE';
    const privateKeyMarker = 'PRIVATE' + ' KEY';
    const p12 = '\\.' + 'p12';
    const pfx = '\\.' + 'pfx';
    return Boolean(value?.trim()) && !/[<>]/.test(value) && !new RegExp(`base64|${certificateMarker}|${privateKeyMarker}|token=|${p12}|${pfx}`, 'i').test(value);
  }
}
