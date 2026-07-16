import { Component, inject, signal } from '@angular/core';
import { VoidedTaxDocumentApiService } from '../../core/services/voided-tax-document-api.service';
import { SanitizationService } from '../../core/services/sanitization.service';
import { TaxDocumentSummary } from '../../core/services/api.models';
import { EmptyStateComponent } from '../../shared/components/empty-state.component';
import { ErrorMessageComponent } from '../../shared/components/error-message.component';
import { FoundationDisclaimerComponent } from '../../shared/components/foundation-disclaimer.component';
import { LoadingStateComponent } from '../../shared/components/loading-state.component';
import { PeriodSelectorComponent } from '../../shared/components/period-selector.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';

@Component({
  standalone: true,
  selector: 'fin-voided-documents',
  imports: [EmptyStateComponent, ErrorMessageComponent, FoundationDisclaimerComponent, LoadingStateComponent, PeriodSelectorComponent, StatusBadgeComponent],
  template: `
    <fin-foundation-disclaimer text="Anulados es read-only en P2. No registra anulaciones ni envía información al SRI." />
    <fin-period-selector [period]="period()" (periodChange)="load($event)" />
    <fin-loading-state [loading]="loading()" />
    <fin-error-message [message]="error()" />
    @if (items().length) {
      <section class="panel">
        <h2>Documentos anulados read-only</h2>
        <table class="table">
          <thead><tr><th>Tipo</th><th>Documento</th><th>Período</th><th>Motivo</th><th>Access key</th><th>Estado</th></tr></thead>
          <tbody>
            @for (item of items(); track item.id) {
              <tr>
                <td>{{ item.documentType || '-' }}</td>
                <td>{{ item.documentNumber || item.establishment + '-' + item.emissionPoint + '-' + item.sequential }}</td>
                <td>{{ item.fiscalPeriod || item.period || period() }}</td>
                <td>{{ item.reason || 'Motivo sanitizado' }}</td>
                <td>{{ accessKey(item) || '-' }}</td>
                <td><fin-status-badge [value]="item.status || 'Foundation'" /></td>
              </tr>
            }
          </tbody>
        </table>
      </section>
    } @else if (!loading()) {
      <fin-empty-state title="Sin anulados para el período" description="Vista read-only. No registra anulaciones desde Angular en P2." />
    }
  `
})
export class VoidedDocumentsComponent {
  private readonly api = inject(VoidedTaxDocumentApiService);
  private readonly sanitizer = inject(SanitizationService);
  protected readonly period = signal(currentPeriod());
  protected readonly items = signal<TaxDocumentSummary[]>([]);
  protected readonly error = signal<string | null>(null);
  protected readonly loading = signal(true);

  constructor() {
    this.load(this.period());
  }

  load(period: string): void {
    this.period.set(period);
    this.loading.set(true);
    this.error.set(null);
    this.api.getByPeriod(period).subscribe({
      next: value => this.items.set(value),
      error: error => this.error.set(error.message),
      complete: () => this.loading.set(false)
    });
  }

  accessKey(item: TaxDocumentSummary): string {
    return item.accessKeyMasked || this.sanitizer.maskAccessKey(item['accessKey'] as string | undefined);
  }
}

function currentPeriod(): string {
  const now = new Date();
  return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`;
}
