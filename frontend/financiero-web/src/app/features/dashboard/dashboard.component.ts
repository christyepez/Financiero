import { Component, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { ContentFileReadinessApiService } from '../../core/services/content-file-readiness-api.service';
import { ExternalApprovalApiService } from '../../core/services/external-approval-api.service';
import { PurchaseTaxDocumentApiService } from '../../core/services/purchase-tax-document-api.service';
import { SriReadinessApiService } from '../../core/services/sri-readiness-api.service';
import { TaxCatalogApiService } from '../../core/services/tax-catalog-api.service';
import { TaxReportingApiService } from '../../core/services/tax-reporting-api.service';
import { VoidedTaxDocumentApiService } from '../../core/services/voided-tax-document-api.service';
import { ErrorMessageComponent } from '../../shared/components/error-message.component';
import { FoundationDisclaimerComponent } from '../../shared/components/foundation-disclaimer.component';
import { LoadingStateComponent } from '../../shared/components/loading-state.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';

@Component({
  standalone: true,
  selector: 'fin-dashboard',
  imports: [ErrorMessageComponent, FoundationDisclaimerComponent, LoadingStateComponent, StatusBadgeComponent],
  template: `
    <fin-foundation-disclaimer />
    <section class="panel">
      <h2>Resumen Sprint 6</h2>
      <p class="muted">Shell Angular listo para pruebas locales, consumo read-only y comandos foundation controlados. No certifica cumplimiento tributario ni habilita flujos productivos.</p>
      <span class="badge info">Portal-ready foundation</span>
      <span class="badge warn">Comandos off por defecto</span>
      <span class="badge bad">Sin SRI producción</span>
    </section>
    <fin-loading-state [loading]="loading()" />
    <fin-error-message [message]="error()" />
    <section class="grid">
      @for (card of cards; track card.title) {
        <article class="card">
          <h3>{{ card.title }}</h3>
          <fin-status-badge [value]="card.status" />
          <p class="muted">{{ card.text }}</p>
        </article>
      }
    </section>
    @if (updatedAt()) { <p class="muted">Última consulta local: {{ updatedAt() }}</p> }
  `
})
export class DashboardComponent {
  private readonly sri = inject(SriReadinessApiService);
  private readonly contentFile = inject(ContentFileReadinessApiService);
  private readonly reporting = inject(TaxReportingApiService);
  private readonly approvals = inject(ExternalApprovalApiService);
  private readonly catalogs = inject(TaxCatalogApiService);
  private readonly purchases = inject(PurchaseTaxDocumentApiService);
  private readonly voided = inject(VoidedTaxDocumentApiService);
  protected readonly period = currentPeriod();
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly updatedAt = signal<string | null>(null);
  protected cards = [
    { title: 'SRI readiness', status: 'Loading', text: 'Consultando backend local.' },
    { title: 'Content/File', status: 'Loading', text: 'Consultando dependency readiness.' },
    { title: 'ATS readiness', status: 'Loading', text: `Período ${this.period}.` },
    { title: 'Aprobaciones externas', status: 'Loading', text: 'Read-only advisory.' },
    { title: 'Catálogos tributarios', status: 'Loading', text: 'Versión foundation.' },
    { title: 'Compras', status: 'Loading', text: '0 registros foundation.' },
    { title: 'Anulados', status: 'Loading', text: '0 registros foundation.' },
    { title: 'Estado', status: 'Foundation', text: 'No productivo. Sin envío SRI, XML completo ni aprobaciones mutables.' }
  ];

  constructor() {
    forkJoin({
      sri: this.sri.getReadiness(),
      contentFile: this.contentFile.getReadiness(),
      ats: this.reporting.getAtsSummary(this.period),
      approvals: this.approvals.getReadiness('all'),
      catalogs: this.catalogs.getAll(),
      purchases: this.purchases.getByPeriod(this.period),
      voided: this.voided.getByPeriod(this.period)
    }).subscribe({
      next: value => {
        this.cards = [
          { title: 'SRI readiness', status: value.sri.status ?? 'Foundation', text: value.sri.disclaimer ?? 'Sin secretos ni URLs privadas completas.' },
          { title: 'Content/File', status: value.contentFile.status ?? value.contentFile.provider ?? 'Foundation', text: value.contentFile.disclaimer ?? 'Storage delegado al Portal.' },
          { title: 'ATS readiness', status: value.ats.status ?? 'Foundation', text: `Período ${value.ats.period ?? this.period}; catálogo ${value.ats.catalogVersion ?? 'foundation'}.` },
          { title: 'Aprobaciones externas', status: value.approvals.status ?? 'Advisory', text: value.approvals.recommendedNextAction ?? 'Revisión externa requerida antes de producción.' },
          { title: 'Catálogos tributarios', status: 'Foundation', text: `Versión ${value.catalogs.version ?? 'foundation'}.` },
          { title: 'Compras', status: 'Read-only', text: `${value.purchases.length} registros foundation para ${this.period}.` },
          { title: 'Anulados', status: 'Read-only', text: `${value.voided.length} registros foundation para ${this.period}.` },
          { title: 'Estado', status: 'Foundation / No productivo', text: 'Sin SRI producción, sin XML completo y con comandos gated por Portal/flags.' }
        ];
        this.updatedAt.set(new Date().toLocaleString());
        this.loading.set(false);
      },
      error: error => {
        this.error.set(error.message);
        this.loading.set(false);
      }
    });
  }
}

function currentPeriod(): string {
  const now = new Date();
  return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`;
}
