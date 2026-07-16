import { Component, inject, signal } from '@angular/core';
import { CatalogItem } from '../../core/services/api.models';
import { TaxCatalogApiService } from '../../core/services/tax-catalog-api.service';
import { EmptyStateComponent } from '../../shared/components/empty-state.component';
import { ErrorMessageComponent } from '../../shared/components/error-message.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';

@Component({
  standalone: true,
  selector: 'fin-tax-catalogs',
  imports: [EmptyStateComponent, ErrorMessageComponent, StatusBadgeComponent],
  template: `
    <fin-error-message [message]="error()" />
    @if (items().length) {
      <section class="panel">
        <h2>Catálogos tributarios foundation</h2>
        <table class="table">
          <thead><tr><th>Código</th><th>Nombre</th><th>Versión</th><th>Estado</th></tr></thead>
          <tbody>
            @for (item of items(); track item.code) {
              <tr>
                <td>{{ item.code || '-' }}</td>
                <td>{{ item.name || item.description || '-' }}</td>
                <td>{{ item.version || 'foundation' }}</td>
                <td><fin-status-badge [value]="item.active === false ? 'Inactive' : 'Active'" /></td>
              </tr>
            }
          </tbody>
        </table>
      </section>
    } @else {
      <fin-empty-state title="Sin catálogos cargados" description="La API puede devolver catálogos foundation cuando exista data local." />
    }
  `
})
export class TaxCatalogsComponent {
  private readonly api = inject(TaxCatalogApiService);
  protected readonly items = signal<CatalogItem[]>([]);
  protected readonly error = signal<string | null>(null);

  constructor() {
    this.api.getAll().subscribe({ next: value => this.items.set(value), error: error => this.error.set(error.message) });
  }
}
