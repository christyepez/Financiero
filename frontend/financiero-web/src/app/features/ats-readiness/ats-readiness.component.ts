import { Component, inject, signal } from '@angular/core';
import { AtsXmlApiService } from '../../core/services/ats-xml-api.service';
import { ReadinessResponse } from '../../core/services/api.models';
import { TaxReportingApiService } from '../../core/services/tax-reporting-api.service';
import { ErrorMessageComponent } from '../../shared/components/error-message.component';
import { FoundationDisclaimerComponent } from '../../shared/components/foundation-disclaimer.component';
import { LoadingStateComponent } from '../../shared/components/loading-state.component';
import { PeriodSelectorComponent } from '../../shared/components/period-selector.component';
import { ReadinessCardComponent } from '../../shared/components/readiness-card.component';

@Component({
  standalone: true,
  selector: 'fin-ats-readiness',
  imports: [ErrorMessageComponent, FoundationDisclaimerComponent, LoadingStateComponent, PeriodSelectorComponent, ReadinessCardComponent],
  template: `
    <fin-foundation-disclaimer text="ATS XML permanece gated. P2 no muestra XML completo ni genera preview oficial." />
    <fin-period-selector [period]="period()" (periodChange)="load($event)" />
    <fin-loading-state [loading]="loading()" />
    <fin-error-message [message]="error()" />
    <section class="grid">
      <fin-readiness-card title="ATS summary" [data]="summary()" />
      <fin-readiness-card title="ATS section readiness" [data]="section()" />
      <fin-readiness-card title="ATS XML gated readiness" [data]="xml()" />
    </section>
  `
})
export class AtsReadinessComponent {
  private readonly reportingApi = inject(TaxReportingApiService);
  private readonly atsApi = inject(AtsXmlApiService);
  protected readonly summary = signal<ReadinessResponse | null>(null);
  protected readonly section = signal<ReadinessResponse | null>(null);
  protected readonly xml = signal<ReadinessResponse | null>(null);
  protected readonly error = signal<string | null>(null);
  protected readonly loading = signal(true);
  protected readonly period = signal(currentPeriod());

  constructor() {
    this.load(this.period());
  }

  load(period: string): void {
    this.period.set(period);
    this.loading.set(true);
    this.error.set(null);
    this.reportingApi.getAtsSummary(period).subscribe({ next: value => this.summary.set(value), error: error => this.error.set(error.message) });
    this.reportingApi.getAtsReadiness(period).subscribe({ next: value => this.section.set(value), error: error => this.error.set(error.message) });
    this.atsApi.getReadiness(period).subscribe({
      next: value => this.xml.set(value),
      error: error => this.error.set(error.message),
      complete: () => this.loading.set(false)
    });
  }
}

function currentPeriod(): string {
  const now = new Date();
  return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`;
}
