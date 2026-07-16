import { Component, inject, signal } from '@angular/core';
import { ExternalApprovalApiService } from '../../core/services/external-approval-api.service';
import { ExternalApprovalGate, ReadinessResponse } from '../../core/services/api.models';
import { ErrorMessageComponent } from '../../shared/components/error-message.component';
import { ReadinessCardComponent } from '../../shared/components/readiness-card.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';

@Component({
  standalone: true,
  selector: 'fin-external-approvals',
  imports: [ErrorMessageComponent, ReadinessCardComponent, StatusBadgeComponent],
  template: `
    <fin-error-message [message]="error()" />
    <section class="stack">
      <fin-readiness-card title="Approval workflow readiness" [data]="readiness()" />
      <div class="panel">
        <h2>Gates externos</h2>
        <table class="table">
          <thead><tr><th>Scope</th><th>Estado</th><th>Mensaje</th></tr></thead>
          <tbody>
            @for (item of approvals(); track item.scope) {
              <tr>
                <td>{{ item.scope || 'all' }}</td>
                <td><fin-status-badge [value]="item.status || (item.approved ? 'Approved' : 'Pending')" /></td>
                <td>{{ item.message || 'Requiere evidencia externa sanitizada.' }}</td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    </section>
  `
})
export class ExternalApprovalsComponent {
  private readonly api = inject(ExternalApprovalApiService);
  protected readonly approvals = signal<ExternalApprovalGate[]>([]);
  protected readonly readiness = signal<ReadinessResponse | null>(null);
  protected readonly error = signal<string | null>(null);

  constructor() {
    this.api.getAll().subscribe({ next: value => this.approvals.set(value), error: error => this.error.set(error.message) });
    this.api.getReadiness('all').subscribe({ next: value => this.readiness.set(value), error: error => this.error.set(error.message) });
  }
}
