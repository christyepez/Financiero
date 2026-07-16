import { Component, Input } from '@angular/core';
import { ReadinessIssue } from '../../core/services/api.models';

@Component({
  selector: 'fin-risk-list',
  standalone: true,
  template: `
    @if (items.length) {
      <ul class="list">
        @for (item of items; track $index) {
          <li>
            <strong>{{ item.code || item.severity || 'issue' }}</strong>
            <div class="muted">{{ item.message || item.field || 'Sin detalle público.' }}</div>
          </li>
        }
      </ul>
    } @else {
      <p class="muted">No hay riesgos reportados por el contrato consultado.</p>
    }
  `
})
export class RiskListComponent {
  @Input() items: ReadinessIssue[] = [];
}
