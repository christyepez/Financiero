import { Component, Input } from '@angular/core';
import { ReadinessResponse } from '../../core/services/api.models';
import { StatusBadgeComponent } from './status-badge.component';
import { RiskListComponent } from './risk-list.component';

@Component({
  selector: 'fin-readiness-card',
  standalone: true,
  imports: [StatusBadgeComponent, RiskListComponent],
  template: `
    <section class="card">
      <h3>{{ title }}</h3>
      <fin-status-badge [value]="status" />
      @if (data?.catalogVersion) { <p><strong>Catalog version:</strong> {{ data?.catalogVersion }}</p> }
      @if (data?.period) { <p><strong>Período:</strong> {{ data?.period }}</p> }
      <p class="muted">{{ data?.disclaimer || fallback }}</p>
      <fin-risk-list [items]="issues" />
    </section>
  `
})
export class ReadinessCardComponent {
  @Input() title = 'Readiness';
  @Input() data: ReadinessResponse | null = null;
  @Input() fallback = 'Contrato foundation consultado sin exponer payloads sensibles.';

  get status(): string {
    if (!this.data) return 'Pending';
    return this.data.status ?? this.data.mode ?? this.data.provider ?? (this.data.isReady || this.data.ready ? 'Ready' : 'Foundation');
  }

  get issues() {
    return [...(this.data?.issues ?? []), ...(this.data?.warnings ?? []), ...(this.data?.blockedReasons ?? [])];
  }
}
