import { Component, inject, signal } from '@angular/core';
import { CatalogItem, TaxCatalogSummary } from '../../core/services/api.models';
import { TaxCatalogApiService } from '../../core/services/tax-catalog-api.service';
import { EmptyStateComponent } from '../../shared/components/empty-state.component';
import { ErrorMessageComponent } from '../../shared/components/error-message.component';
import { FoundationDisclaimerComponent } from '../../shared/components/foundation-disclaimer.component';
import { LoadingStateComponent } from '../../shared/components/loading-state.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';

@Component({
  standalone: true,
  selector: 'fin-tax-catalogs',
  imports: [EmptyStateComponent, ErrorMessageComponent, FoundationDisclaimerComponent, LoadingStateComponent, StatusBadgeComponent],
  template: `
    <fin-foundation-disclaimer text="Catálogos foundation sujetos a revisión tributaria. No sustituyen catálogo oficial validado." />
    <div class="panel">
      <span class="badge warn">Catálogo foundation</span>
      <span class="badge bad">Revisión tributaria pendiente</span>
      <p class="muted">Los códigos se muestran como referencia técnica; no afirman vigencia oficial ni cobertura completa.</p>
    </div>
    <fin-loading-state [loading]="loading()" />
    <fin-error-message [message]="error()" />
    @if (summary()) {
      <section class="panel">
        <h2>Catálogos tributarios foundation</h2>
        <p class="muted">Versión: {{ summary()?.version || 'foundation' }} · {{ summary()?.disclaimer || 'Requiere revisión tributaria.' }}</p>
        @for (group of groups(); track group.title) {
          <h3>{{ group.title }}</h3>
        <table class="table">
          <thead><tr><th>Código</th><th>Nombre</th><th>Versión</th><th>Badges</th></tr></thead>
          <tbody>
            @for (item of group.items; track item.code) {
              <tr>
                <td>{{ item.code || '-' }}</td>
                <td>{{ item.name || item.description || '-' }}</td>
                <td>{{ item.version || 'foundation' }}</td>
                <td>
                  <fin-status-badge [value]="item.requiresTaxReview ? 'Requires tax review' : 'Foundation'" />
                  <fin-status-badge [value]="item.appliesToAts ? 'ATS' : 'Reference'" />
                </td>
              </tr>
            }
          </tbody>
        </table>
        }
      </section>
    } @else if (!loading()) {
      <fin-empty-state title="Sin catálogos cargados" description="La API puede devolver catálogos foundation cuando exista data local." />
    }
  `
})
export class TaxCatalogsComponent {
  private readonly api = inject(TaxCatalogApiService);
  protected readonly summary = signal<TaxCatalogSummary | null>(null);
  protected readonly error = signal<string | null>(null);
  protected readonly loading = signal(true);

  constructor() {
    this.api.getAll().subscribe({
      next: value => this.summary.set(value),
      error: error => this.error.set(error.message),
      complete: () => this.loading.set(false)
    });
  }

  groups(): { title: string; items: CatalogItem[] }[] {
    const value = this.summary();
    return [
      { title: 'Tipos de documento de compra', items: value?.purchaseDocumentTypes ?? [] },
      { title: 'Sustentos tributarios', items: value?.supportDocumentTypes ?? [] },
      { title: 'Tipos de anulados', items: value?.voidedDocumentTypes ?? [] },
      { title: 'Códigos de impuesto en compras', items: value?.purchaseTaxCodes ?? [] },
      { title: 'Identificación de proveedor', items: value?.supplierIdentificationTypes ?? [] }
    ].filter(x => x.items.length > 0);
  }
}
