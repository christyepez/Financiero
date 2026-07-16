import { Component, inject, signal } from '@angular/core';
import { PurchaseTaxDocumentApiService } from '../../core/services/purchase-tax-document-api.service';
import { TaxDocumentSummary } from '../../core/services/api.models';
import { EmptyStateComponent } from '../../shared/components/empty-state.component';
import { ErrorMessageComponent } from '../../shared/components/error-message.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';

@Component({
  standalone: true,
  selector: 'fin-purchases',
  imports: [EmptyStateComponent, ErrorMessageComponent, StatusBadgeComponent],
  template: `
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
                <td>{{ item.supplierName || 'Proveedor sanitizado' }}</td>
                <td>{{ item.period || period }}</td>
                <td>{{ item.total ?? '-' }}</td>
                <td><fin-status-badge [value]="item.status || 'Foundation'" /></td>
              </tr>
            }
          </tbody>
        </table>
      </section>
    } @else {
      <fin-empty-state title="Sin compras para el período" description="Vista read-only. No crea ni edita compras desde Angular en P1." />
    }
  `
})
export class PurchasesComponent {
  private readonly api = inject(PurchaseTaxDocumentApiService);
  protected readonly period = '2026-01';
  protected readonly items = signal<TaxDocumentSummary[]>([]);
  protected readonly error = signal<string | null>(null);

  constructor() {
    this.api.getByPeriod(this.period).subscribe({ next: value => this.items.set(value), error: error => this.error.set(error.message) });
  }
}
