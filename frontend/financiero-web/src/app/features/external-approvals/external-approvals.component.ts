import { Component, inject, signal } from '@angular/core';
import { ExternalApprovalApiService } from '../../core/services/external-approval-api.service';
import { ExternalApprovalGate, ReadinessResponse } from '../../core/services/api.models';
import { ErrorMessageComponent } from '../../shared/components/error-message.component';
import { EmptyStateComponent } from '../../shared/components/empty-state.component';
import { FoundationDisclaimerComponent } from '../../shared/components/foundation-disclaimer.component';
import { LoadingStateComponent } from '../../shared/components/loading-state.component';
import { ReadinessCardComponent } from '../../shared/components/readiness-card.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';

@Component({
  standalone: true,
  selector: 'fin-external-approvals',
  imports: [EmptyStateComponent, ErrorMessageComponent, FoundationDisclaimerComponent, LoadingStateComponent, ReadinessCardComponent, StatusBadgeComponent],
  template: `
    <fin-foundation-disclaimer text="Aprobaciones externas son advisory/read-only. P2 no aprueba, no persiste evidencia y no habilita producción." />
    <div class="panel">
      <span class="badge info">Read-only advisory</span>
      <span class="badge warn">Evidencia fuera del repo</span>
      <p class="muted">Use esta vista como checklist de preparación. La aprobación real requiere revisión externa y contrato Portal definido.</p>
    </div>
    <fin-loading-state [loading]="loading()" />
    <fin-error-message [message]="error()" />
    <section class="stack">
      <fin-readiness-card title="Approval workflow readiness" [data]="readiness()" />
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
  protected readonly approvals = signal<ExternalApprovalGate[]>([]);
  protected readonly readiness = signal<ReadinessResponse | null>(null);
  protected readonly error = signal<string | null>(null);
  protected readonly loading = signal(true);

  constructor() {
    this.api.getAll().subscribe({ next: value => this.approvals.set(value), error: error => this.error.set(error.message) });
    this.api.getReadiness('all').subscribe({
      next: value => this.readiness.set(value),
      error: error => this.error.set(error.message),
      complete: () => this.loading.set(false)
    });
  }
}
