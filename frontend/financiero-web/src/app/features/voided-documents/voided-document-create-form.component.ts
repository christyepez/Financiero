import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RegisterVoidedTaxDocumentRequest, TaxDocumentSummary } from '../../core/services/api.models';
import { VoidedTaxDocumentApiService } from '../../core/services/voided-tax-document-api.service';
import { SanitizationService } from '../../core/services/sanitization.service';
import { CommandResultSummaryComponent } from '../../shared/components/command-result-summary.component';
import { ErrorMessageComponent } from '../../shared/components/error-message.component';
import { FoundationCommandDisclaimerComponent } from '../../shared/components/foundation-command-disclaimer.component';
import { ValidationIssuesListComponent } from '../../shared/components/validation-issues-list.component';

@Component({
  selector: 'fin-voided-document-create-form',
  standalone: true,
  imports: [CommandResultSummaryComponent, ErrorMessageComponent, FormsModule, FoundationCommandDisclaimerComponent, ValidationIssuesListComponent],
  template: `
    @if (enabled) {
      <section class="panel">
        <h2>Registrar anulado foundation</h2>
        <fin-foundation-command-disclaimer />
        <fin-error-message [message]="error" />
        <fin-validation-issues-list [issues]="issues" />
        <form class="form-grid" (ngSubmit)="submit()">
          <input name="documentType" [(ngModel)]="model.documentType" placeholder="Tipo doc" required />
          <input name="establishment" [(ngModel)]="model.establishment" placeholder="Estab." required />
          <input name="emissionPoint" [(ngModel)]="model.emissionPoint" placeholder="Pto emi" required />
          <input name="sequential" [(ngModel)]="model.sequential" placeholder="Secuencial" required />
          <input name="issueDate" [(ngModel)]="model.issueDate" type="date" required />
          <input name="voidDate" [(ngModel)]="model.voidDate" type="date" required />
          <input name="fiscalPeriod" [(ngModel)]="model.fiscalPeriod" placeholder="YYYY-MM" required />
          <input name="reason" [(ngModel)]="model.reason" placeholder="Motivo foundation" required />
          <input name="authorizationNumber" [(ngModel)]="model.authorizationNumber" placeholder="Autorización opcional" />
          <input name="accessKey" [(ngModel)]="model.accessKey" placeholder="AccessKey opcional" />
          <button type="submit">Registrar foundation</button>
        </form>
        <fin-command-result-summary [result]="result" />
      </section>
    }
  `
})
export class VoidedDocumentCreateFormComponent {
  private readonly api = inject(VoidedTaxDocumentApiService);
  private readonly sanitizer = inject(SanitizationService);
  @Input() enabled = false;
  @Input() period = currentPeriod();
  @Output() registered = new EventEmitter<TaxDocumentSummary>();
  protected error: string | null = null;
  protected issues: string[] = [];
  protected result: TaxDocumentSummary | null = null;
  protected model: RegisterVoidedTaxDocumentRequest = defaultVoided(currentPeriod());

  submit(): void {
    this.issues = validateVoided(this.model);
    if (this.issues.length) return;
    if (!confirm('Registrar anulado foundation. No se enviará al SRI ni constituye anulación oficial. ¿Continuar?')) return;
    const request = { ...this.model, reason: this.sanitizer.safeText(this.model.reason), accessKey: this.sanitizer.safeText(this.model.accessKey) };
    this.api.registerVoidedDocument(request).subscribe({
      next: value => {
        this.result = value;
        this.registered.emit(value);
      },
      error: error => this.error = error.message
    });
  }
}

function defaultVoided(period: string): RegisterVoidedTaxDocumentRequest {
  const date = `${period}-01`;
  return {
    documentType: '01',
    establishment: '001',
    emissionPoint: '001',
    sequential: `${Date.now()}`.slice(-9).padStart(9, '0'),
    issueDate: date,
    voidDate: date,
    fiscalPeriod: period,
    reason: 'Registro foundation sintético'
  };
}

function validateVoided(model: RegisterVoidedTaxDocumentRequest): string[] {
  const issues = [];
  if (!/^\d{3}$/.test(model.establishment)) issues.push('Establecimiento debe tener 3 dígitos.');
  if (!/^\d{3}$/.test(model.emissionPoint)) issues.push('Punto de emisión debe tener 3 dígitos.');
  if (!/^\d{9}$/.test(model.sequential)) issues.push('Secuencial debe tener 9 dígitos.');
  if (!/^\d{4}-\d{2}$/.test(model.fiscalPeriod)) issues.push('Periodo fiscal debe usar YYYY-MM.');
  if (!model.reason.trim()) issues.push('Motivo es requerido.');
  return issues;
}

function currentPeriod(): string {
  const now = new Date();
  return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`;
}
