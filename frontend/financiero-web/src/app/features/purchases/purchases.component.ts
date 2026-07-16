import { Component, inject, signal } from '@angular/core';
import { PurchaseTaxDocumentApiService } from '../../core/services/purchase-tax-document-api.service';
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
  selector: 'fin-purchases',
  imports: [EmptyStateComponent, ErrorMessageComponent, FoundationDisclaimerComponent, LoadingStateComponent, PeriodSelectorComponent, StatusBadgeComponent],
  template: `
    <fin-foundation-disclaimer text="Compras es read-only en P2. No crea, edita ni contabiliza documentos." />
    <fin-period-selector [period]="period()" (periodChange)="load($event)" />
    <fin-loading-state [loading]="loading()" />
    <fin-error-message [message]="error()" />
    @if (items().length) {
      <section class="panel">
        <h2>Compras tributarias read-only</h2>
        <table class="table">
          <thead><tr><th>Tipo</th><th>Proveedor</th><th>Período</th><th>Total</th><th>Estado</th></tr></thead>
          <tbody>
            @for (item of items(); track item.id) {
              <tr>
                <td>{{ item.documentType || '-' }}</td>
                <td>{{ item.supplierName || 'Proveedor sanitizado' }} · {{ supplierId(item) }}</td>
                <td>{{ item.fiscalPeriod || item.period || period() }}</td>
                <td>{{ item.total ?? '-' }}</td>
                <td><fin-status-badge [value]="item.status || 'Foundation'" /></td>
              </tr>
            }
          </tbody>
        </table>
      </section>
    } @else if (!loading()) {
      <fin-empty-state title="Sin compras para el período" description="Vista read-only. No crea ni edita compras desde Angular en P2." />
    }
  `
})
export class PurchasesComponent {
  private readonly api = inject(PurchaseTaxDocumentApiService);
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

  supplierId(item: TaxDocumentSummary): string {
    return item.supplierIdentificationMasked || this.sanitizer.maskIdentifier(item['supplierIdentification'] as string | undefined);
  }
}

function currentPeriod(): string {
  const now = new Date();
  return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`;
}
