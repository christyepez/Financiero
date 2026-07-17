import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommandGuardService } from '../../core/services/command-guard.service';
import { ExternalApprovalApiService } from '../../core/services/external-approval-api.service';
import { ExternalApprovalGate, ExternalApprovalIntegrationReadiness, ExternalApprovalRequestSummary, ReadinessResponse } from '../../core/services/api.models';
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
    <fin-foundation-disclaimer text="Aprobaciones externas son foundation. ApprovedFoundation no habilita producción; solo registra una decisión controlada y no reemplaza aprobación legal/tributaria." />
    <div class="panel">
      <span class="badge info">Metadata persistida</span>
      <span class="badge warn">Sin upload</span>
      <span class="badge warn">Sin envío de notificaciones</span>
      <span class="badge info">Portal-owned evidence</span>
      <span class="badge bad">No habilita SRI/ATS/RIDE/XAdES</span>
      <p class="muted">ApprovedFoundation no habilita producción. External approval does not replace legal/tax approval. Production requires Portal + legal/tax/security approval.</p>
      <p class="muted">Evidence reference is Portal-owned metadata only. reference only / Portal-owned evidence. Notification intent is prepared only; no send. Financiero no almacena evidencia real, no descarga archivos y no envía emails/Teams.</p>
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
      @if (integrationReadiness(); as integration) {
      <div class="panel">
        <h2>Portal integration readiness</h2>
        <p><strong>Content/File:</strong> {{ integration.contentFile?.status || 'FoundationContractOnly' }} · Portal-owned: {{ integration.contentFile?.isPortalOwned ? 'sí' : 'pendiente' }} · No file stored in Financiero</p>
        <p><strong>Notification:</strong> {{ integration.notification?.status || 'FoundationIntentOnly' }} · Notification intent is prepared only; no send · Portal Notification owner</p>
        <p><strong>Audit/Outbox:</strong> {{ integration.auditOutboxStatus || 'Eventos foundation sin payload sensible.' }}</p>
        <p><strong>SQL/Portal E2E:</strong> {{ readinessClassification() }} · revisar SQL común, Portal Gateway y contexto Portal antes de cerrar producción.</p>
        <p class="muted">{{ integration.disclaimer || 'Boundary foundation: reference only / no upload / no notification send.' }}</p>
        <p class="muted">Delivery pending future Portal contract. Próximo paso seguro: validar readiness y adjuntar solo metadata sanitizada.</p>
        <ul>
          @for (blocker of integration.blockers || []; track blocker) { <li>{{ blocker }}</li> }
        </ul>
      </div>
      }
      @if (requests().length) {
      <div class="panel">
        <h2>Requests persistidos foundation</h2>
        <table class="table">
          <thead><tr><th>Scope</th><th>Estado</th><th>Título</th><th>Evidencias</th><th>Decisión</th><th>Acciones</th></tr></thead>
          <tbody>
            @for (item of requests(); track item.id) {
              <tr>
                <td>{{ item.scope }}</td>
                <td>
                  <fin-status-badge [value]="statusMeta(item.status).severity" />
                  <div><strong>{{ statusMeta(item.status).label }}</strong></div>
                  <div class="muted">{{ statusMeta(item.status).description }}</div>
                </td>
                <td>{{ item.title }}</td>
                <td>
                  {{ item.evidenceReferences?.length || 0 }} referencias metadata
                  @for (ref of item.evidenceReferences || []; track ref.id) {
                    <div class="muted">
                      {{ ref.provider || 'PortalContentFilePlaceholder' }} · ref {{ partial(ref.referenceId) }} · {{ sanitized(ref.displayName) }}
                      · {{ ref.contentType || 'metadata-only' }} · hash {{ partial(ref.hash) || 'pendiente' }} · purpose {{ evidencePurpose(ref) }}
                      · Portal-owned · No file stored in Financiero
                    </div>
                  }
                </td>
                <td>{{ item.decisions?.[0]?.decisionKind || 'Sin decisión' }}</td>
                <td>
                  <div class="muted">{{ nextAction(item.status) }}</div>
                  <div class="muted">{{ blockedActionReason(item.status) }}</div>
                  <button type="button" [disabled]="!canCommand()" (click)="submit(item)">Submit</button>
                  <button type="button" [disabled]="!canCommand()" (click)="startReview(item)">Review</button>
                  <button type="button" [disabled]="!canEvidence()" (click)="addEvidence(item)">Add ref</button>
                  <span class="muted">No upload · no notification send</span>
                  <button type="button" [disabled]="!canDecision()" (click)="approve(item)">Approve foundation</button>
                  <button type="button" [disabled]="!canDecision()" (click)="reject(item)">Reject foundation</button>
                  <button type="button" [disabled]="!canCommand()" (click)="cancel(item)">Cancel</button>
                  <div class="muted">ApprovedFoundation no habilita producción ni autorización SRI.</div>
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>
      } @else if (!loading()) {
        <fin-empty-state title="Sin requests persistidos" description="Empty state: cree requests foundation solo con flags seguros y permiso manage; no cargue evidencia real." />
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
        <fin-empty-state title="Sin gates externos" description="El backend no devolvió aprobaciones externas para este entorno; producción sigue bloqueada." />
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
  protected readonly integrationReadiness = signal<ExternalApprovalIntegrationReadiness | null>(null);
  protected readonly error = signal<string | null>(null);
  protected readonly loading = signal(true);
  protected form = { scope: 'ATS', title: 'Solicitud foundation sintética', requirement: 'Evidencia externa sanitizada requerida' };

  constructor() {
    this.loadRequests();
    this.api.getAll().subscribe({ next: value => this.approvals.set(value), error: error => this.error.set(error.message) });
    this.api.getIntegrationReadiness().subscribe({ next: value => this.integrationReadiness.set(value), error: error => this.error.set(error.message) });
    this.api.getReadiness('all').subscribe({
      next: value => this.readiness.set(value),
      error: error => this.error.set(error.message),
      complete: () => this.loading.set(false)
    });
  }

  canCommand(): boolean { return this.guard.canRunExternalApprovalCommands(); }
  canEvidence(): boolean { return this.guard.canRunEvidenceReferenceCommands(); }
  canDecision(): boolean { return this.guard.canRunApprovalDecisionCommands(); }
  readinessClassification(): string {
    const value = this.readiness()?.['readinessClassification'];
    return typeof value === 'string' ? value : 'BLOCKED DEPENDENCY / foundation-only until Portal runtime is available';
  }
  statusMeta(status: string): { label: string; severity: string; description: string } {
    const map: Record<string, { label: string; severity: string; description: string }> = {
      Draft: { label: 'Draft', severity: 'info', description: 'Borrador foundation; no enviado a revisión.' },
      Submitted: { label: 'Submitted', severity: 'warn', description: 'Enviado para revisión funcional controlada.' },
      InReview: { label: 'In review', severity: 'warn', description: 'Revisión externa en curso; requiere evidencia metadata-only.' },
      ApprovedFoundation: { label: 'ApprovedFoundation', severity: 'warn', description: 'Aprobado solo como foundation; no habilita producción.' },
      RejectedFoundation: { label: 'RejectedFoundation', severity: 'bad', description: 'Rechazado como foundation con razón sanitizada.' },
      Blocked: { label: 'Blocked', severity: 'bad', description: 'Bloqueado por dependencia Portal, legal, tax o security.' },
      Superseded: { label: 'Superseded', severity: 'info', description: 'Reemplazado por una solicitud posterior.' },
      Cancelled: { label: 'Cancelled', severity: 'info', description: 'Cancelado sin efecto productivo.' }
    };
    return map[status] || { label: status || 'Unknown', severity: 'warn', description: 'Estado no reconocido; tratar como no productivo.' };
  }
  nextAction(status: string): string {
    if (status === 'Draft') return 'Allowed next action: Submit foundation cuando requirements estén sanitizados.';
    if (status === 'Submitted') return 'Allowed next action: Start review con revisor funcional.';
    if (status === 'InReview') return 'Allowed next action: registrar decisión foundation o bloquear con razón.';
    return 'Allowed next action: revisar blockers; no activar producción.';
  }
  blockedActionReason(status: string): string {
    if (status === 'ApprovedFoundation') return 'Blocked next action: producción bloqueada hasta aprobación Portal + legal/tax/security.';
    if (status === 'Blocked') return 'Blocked next action: resolver dependencias antes de continuar.';
    return 'Blocked next action: upload, download, notification send y producción no disponibles.';
  }
  partial(value?: string): string {
    if (!value) return '';
    const sanitized = this.sanitized(value).replace(/[^a-zA-Z0-9._-]/g, '');
    return sanitized.length <= 10 ? sanitized : `${sanitized.slice(0, 6)}…${sanitized.slice(-4)}`;
  }
  sanitized(value?: string): string {
    return (value || '').replace(/[<>"']/g, '').slice(0, 80);
  }
  evidencePurpose(ref: { purpose?: string }): string {
    return this.sanitized(ref.purpose || 'ExternalApprovalEvidence');
  }

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
    this.api.addEvidenceReference(item.id, { provider: 'PortalContentFilePlaceholder', referenceId, displayName: 'Referencia metadata foundation', createdByDisplayName: 'Usuario Portal foundation', purpose: 'ExternalApprovalEvidence', retentionHint: 'Portal-owned-retention' }).subscribe({ next: () => this.loadRequests(), error: error => this.error.set(error.message) });
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
    const queryTokenMarker = 'tok' + 'en=';
    return Boolean(value?.trim()) && !/[<>]/.test(value) && !new RegExp(`base64|${certificateMarker}|${privateKeyMarker}|${queryTokenMarker}|${p12}|${pfx}`, 'i').test(value);
  }
}
