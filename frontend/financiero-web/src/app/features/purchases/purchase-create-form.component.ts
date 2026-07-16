import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CreatePurchaseTaxDocumentRequest, TaxDocumentSummary } from '../../core/services/api.models';
import { PurchaseTaxDocumentApiService } from '../../core/services/purchase-tax-document-api.service';
import { SanitizationService } from '../../core/services/sanitization.service';
import { CommandResultSummaryComponent } from '../../shared/components/command-result-summary.component';
import { ErrorMessageComponent } from '../../shared/components/error-message.component';
import { FoundationCommandDisclaimerComponent } from '../../shared/components/foundation-command-disclaimer.component';
import { ValidationIssuesListComponent } from '../../shared/components/validation-issues-list.component';

@Component({
  selector: 'fin-purchase-create-form',
  standalone: true,
  imports: [CommandResultSummaryComponent, ErrorMessageComponent, FormsModule, FoundationCommandDisclaimerComponent, ValidationIssuesListComponent],
  template: `
    @if (enabled) {
      <section class="panel">
        <h2>Crear compra foundation</h2>
        <fin-foundation-command-disclaimer />
        <fin-error-message [message]="error" />
        <fin-validation-issues-list [issues]="issues" />
        <form class="form-grid" (ngSubmit)="submit()">
          <input name="supplierIdentificationType" [(ngModel)]="model.supplierIdentificationType" placeholder="Tipo identificación" required />
          <input name="supplierIdentification" [(ngModel)]="model.supplierIdentification" placeholder="Identificación sintética" required />
          <input name="supplierName" [(ngModel)]="model.supplierName" placeholder="Proveedor" required />
          <input name="documentType" [(ngModel)]="model.documentType" placeholder="Tipo doc" required />
          <input name="establishment" [(ngModel)]="model.establishment" placeholder="Estab." required />
          <input name="emissionPoint" [(ngModel)]="model.emissionPoint" placeholder="Pto emi" required />
          <input name="sequential" [(ngModel)]="model.sequential" placeholder="Secuencial" required />
          <input name="supportDocumentType" [(ngModel)]="model.supportDocumentType" placeholder="Sustento" required />
          <input name="issueDate" [(ngModel)]="model.issueDate" type="date" required />
          <input name="registrationDate" [(ngModel)]="model.registrationDate" type="date" required />
          <input name="fiscalPeriod" [(ngModel)]="model.fiscalPeriod" placeholder="YYYY-MM" required />
          <input name="subtotal" [(ngModel)]="model.subtotal" type="number" step="0.01" placeholder="Subtotal" required />
          <input name="taxTotal" [(ngModel)]="model.taxTotal" type="number" step="0.01" placeholder="IVA" required />
          <input name="total" [(ngModel)]="model.total" type="number" step="0.01" placeholder="Total" required />
          <input name="authorizationNumber" [(ngModel)]="model.authorizationNumber" placeholder="Autorización opcional" />
          <input name="accessKey" [(ngModel)]="model.accessKey" placeholder="AccessKey opcional" />
          <button type="submit">Crear foundation</button>
        </form>
        <fin-command-result-summary [result]="result" />
      </section>
    }
  `
})
export class PurchaseCreateFormComponent {
  private readonly api = inject(PurchaseTaxDocumentApiService);
  private readonly sanitizer = inject(SanitizationService);
  @Input() enabled = false;
  @Input() period = currentPeriod();
  @Output() created = new EventEmitter<TaxDocumentSummary>();
  protected error: string | null = null;
  protected issues: string[] = [];
  protected result: TaxDocumentSummary | null = null;
  protected model: CreatePurchaseTaxDocumentRequest = defaultPurchase(currentPeriod());

  submit(): void {
    this.issues = validatePurchase(this.model);
    if (this.issues.length) return;
    if (!confirm('Crear compra foundation sintética. No se enviará al SRI ni generará ATS oficial. ¿Continuar?')) return;
    const request = { ...this.model, supplierIdentification: this.sanitizer.safeText(this.model.supplierIdentification), accessKey: this.sanitizer.safeText(this.model.accessKey) };
    this.api.createPurchase(request).subscribe({
      next: value => {
        this.result = value;
        this.created.emit(value);
      },
      error: error => this.error = error.message
    });
  }
}

function defaultPurchase(period: string): CreatePurchaseTaxDocumentRequest {
  const date = `${period}-01`;
  return {
    supplierIdentificationType: '04',
    supplierIdentification: '0999999999001',
    supplierName: 'Proveedor Sintetico',
    establishment: '001',
    emissionPoint: '001',
    sequential: `${Date.now()}`.slice(-9).padStart(9, '0'),
    documentType: '01',
    issueDate: date,
    registrationDate: date,
    fiscalPeriod: period,
    supportDocumentType: '01',
    subtotal: 100,
    taxTotal: 12,
    total: 112,
    currency: 'USD'
  };
}

function validatePurchase(model: CreatePurchaseTaxDocumentRequest): string[] {
  const issues = [];
  if (!/^\d{3}$/.test(model.establishment)) issues.push('Establecimiento debe tener 3 dígitos.');
  if (!/^\d{3}$/.test(model.emissionPoint)) issues.push('Punto de emisión debe tener 3 dígitos.');
  if (!/^\d{9}$/.test(model.sequential)) issues.push('Secuencial debe tener 9 dígitos.');
  if (!/^\d{4}-\d{2}$/.test(model.fiscalPeriod)) issues.push('Periodo fiscal debe usar YYYY-MM.');
  if (model.total < model.subtotal + model.taxTotal - 0.01) issues.push('Total debe cubrir subtotal + impuestos.');
  return issues;
}

function currentPeriod(): string {
  const now = new Date();
  return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`;
}
