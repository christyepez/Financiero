import { Component, inject, signal } from '@angular/core';
import { AtsXmlApiService } from '../../core/services/ats-xml-api.service';
import { ReadinessResponse } from '../../core/services/api.models';
import { TaxReportingApiService } from '../../core/services/tax-reporting-api.service';
import { ErrorMessageComponent } from '../../shared/components/error-message.component';
import { ReadinessCardComponent } from '../../shared/components/readiness-card.component';

@Component({
  standalone: true,
  selector: 'fin-ats-readiness',
  imports: [ErrorMessageComponent, ReadinessCardComponent],
  template: `
    <fin-error-message [message]="error()" />
    <section class="grid">
      <fin-readiness-card title="ATS section readiness" [data]="section()" />
      <fin-readiness-card title="ATS XML gated readiness" [data]="xml()" />
    </section>
  `
})
export class AtsReadinessComponent {
  private readonly reportingApi = inject(TaxReportingApiService);
  private readonly atsApi = inject(AtsXmlApiService);
  protected readonly section = signal<ReadinessResponse | null>(null);
  protected readonly xml = signal<ReadinessResponse | null>(null);
  protected readonly error = signal<string | null>(null);
  private readonly period = '2026-01';

  constructor() {
    this.reportingApi.getAtsReadiness(this.period).subscribe({ next: value => this.section.set(value), error: error => this.error.set(error.message) });
    this.atsApi.getReadiness(this.period).subscribe({ next: value => this.xml.set(value), error: error => this.error.set(error.message) });
  }
}
