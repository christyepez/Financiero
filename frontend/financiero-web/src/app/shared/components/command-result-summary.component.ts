import { Component, Input } from '@angular/core';
import { TaxDocumentSummary } from '../../core/services/api.models';
import { StatusBadgeComponent } from './status-badge.component';

@Component({
  selector: 'fin-command-result-summary',
  standalone: true,
  imports: [StatusBadgeComponent],
  template: `
    @if (result) {
      <section class="card">
        <h3>Resultado foundation</h3>
        <fin-status-badge [value]="result.status || 'Foundation'" />
        <p class="muted">{{ result.disclaimer || 'Resultado sanitizado. No certifica cumplimiento tributario oficial.' }}</p>
        <ul class="list">
          <li>Documento: {{ result.documentNumber || result.documentType || '-' }}</li>
          <li>Identificación/AccessKey: {{ result.supplierIdentificationMasked || result.accessKeyMasked || '-' }}</li>
          <li>Total: {{ result.total ?? '-' }}</li>
        </ul>
      </section>
    }
  `
})
export class CommandResultSummaryComponent {
  @Input() result: TaxDocumentSummary | null = null;
}
