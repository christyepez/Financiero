import { Component, Input } from '@angular/core';
import { TaxDocumentSummary } from '../../core/services/api.models';

@Component({
  selector: 'fin-purchase-tax-summary',
  standalone: true,
  template: `
    <section class="card">
      <h3>Resumen compras</h3>
      <p>Total documentos: {{ items.length }}</p>
      <p>Subtotal: {{ subtotal }}</p>
      <p>Impuestos: {{ taxTotal }}</p>
      <p>Total: {{ total }}</p>
    </section>
  `
})
export class PurchaseTaxSummaryComponent {
  @Input() items: TaxDocumentSummary[] = [];

  get subtotal(): number { return this.items.reduce((sum, item) => sum + Number(item.subtotal ?? 0), 0); }
  get taxTotal(): number { return this.items.reduce((sum, item) => sum + Number(item.taxTotal ?? 0), 0); }
  get total(): number { return this.items.reduce((sum, item) => sum + Number(item.total ?? 0), 0); }
}
