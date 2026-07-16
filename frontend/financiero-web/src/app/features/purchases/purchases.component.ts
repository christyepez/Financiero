import { Component, inject, signal } from '@angular/core';
import { CommandGuardService } from '../../core/services/command-guard.service';
import { PurchaseTaxDocumentApiService } from '../../core/services/purchase-tax-document-api.service';
import { SanitizationService } from '../../core/services/sanitization.service';
import { TaxDocumentSummary } from '../../core/services/api.models';
import { CommandDisabledBannerComponent } from '../../shared/components/command-disabled-banner.component';
import { EmptyStateComponent } from '../../shared/components/empty-state.component';
import { ErrorMessageComponent } from '../../shared/components/error-message.component';
import { FoundationDisclaimerComponent } from '../../shared/components/foundation-disclaimer.component';
import { LoadingStateComponent } from '../../shared/components/loading-state.component';
import { PeriodSelectorComponent } from '../../shared/components/period-selector.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';
import { PurchaseCreateFormComponent } from './purchase-create-form.component';
import { PurchaseTaxSummaryComponent } from './purchase-tax-summary.component';

@Component({
  standalone: true,
  selector: 'fin-purchases',
  imports: [CommandDisabledBannerComponent, EmptyStateComponent, ErrorMessageComponent, FoundationDisclaimerComponent, LoadingStateComponent, PeriodSelectorComponent, PurchaseCreateFormComponent, PurchaseTaxSummaryComponent, StatusBadgeComponent],
  template: `
    <fin-foundation-disclaimer text="Compras foundation. Los comandos son internos, no oficiales y no envían datos al SRI." />
    <div class="panel">
      <span class="badge warn">Comando foundation</span>
      <span class="badge info">Requiere permiso Portal</span>
      <p class="muted">Crear/validar compras aquí solo prepara datos sintéticos o locales. No genera ATS oficial ni contabilización automática.</p>
    </div>
    @if (!canCommand()) {
      <fin-command-disabled-banner [reason]="guard.disabledReason('purchase')" />
    }
    <fin-purchase-create-form [enabled]="canCommand()" [period]="period()" (created)="afterCommand($event)" />
    <fin-period-selector [period]="period()" (periodChange)="load($event)" />
    <fin-loading-state [loading]="loading()" />
    <fin-error-message [message]="error()" />
    <fin-purchase-tax-summary [items]="items()" />
    @if (items().length) {
      <section class="panel">
        <h2>Compras tributarias foundation</h2>
        <table class="table">
          <thead><tr><th>Tipo</th><th>Proveedor</th><th>Período</th><th>Total</th><th>Estado</th><th>Acción</th></tr></thead>
          <tbody>
            @for (item of items(); track item.id) {
              <tr>
                <td>{{ item.documentType || '-' }}</td>
                <td>{{ item.supplierName || 'Proveedor sanitizado' }} · {{ supplierId(item) }}</td>
                <td>{{ item.fiscalPeriod || item.period || period() }}</td>
                <td>{{ item.total ?? '-' }}</td>
                <td><fin-status-badge [value]="item.status || 'Foundation'" /></td>
                <td>
                  <button type="button" [disabled]="!canCommand()" (click)="validate(item)">Validar foundation</button>
                </td>
              </tr>
            }
          </tbody>
        </table>
      </section>
    } @else if (!loading()) {
      <fin-empty-state title="Sin compras para el período" description="No hay compras foundation para el período seleccionado. Los comandos permanecen apagados salvo flags y permisos explícitos." />
    }
  `
})
export class PurchasesComponent {
  private readonly api = inject(PurchaseTaxDocumentApiService);
  protected readonly guard = inject(CommandGuardService);
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

  canCommand(): boolean {
    return this.guard.canRunPurchaseCommands();
  }

  afterCommand(_: TaxDocumentSummary): void {
    this.load(this.period());
  }

  validate(item: TaxDocumentSummary): void {
    if (!item.id || !this.canCommand()) return;
    if (!confirm('Validar compra foundation. No certifica cumplimiento tributario oficial. ¿Continuar?')) return;
    this.api.validatePurchase(item.id).subscribe({
      next: value => {
        this.items.update(items => items.map(current => current.id === value.id ? value : current));
      },
      error: error => this.error.set(error.message)
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
