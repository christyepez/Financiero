import { Component, inject, signal } from '@angular/core';
import { ContentFileReadinessApiService } from '../../core/services/content-file-readiness-api.service';
import { ReadinessResponse } from '../../core/services/api.models';
import { SriReadinessApiService } from '../../core/services/sri-readiness-api.service';
import { ErrorMessageComponent } from '../../shared/components/error-message.component';
import { ReadinessCardComponent } from '../../shared/components/readiness-card.component';

@Component({
  standalone: true,
  selector: 'fin-sri-readiness',
  imports: [ErrorMessageComponent, ReadinessCardComponent],
  template: `
    <fin-error-message [message]="error()" />
    <section class="grid">
      <fin-readiness-card title="SRI Test/Mock readiness" [data]="sri()" />
      <fin-readiness-card title="Portal Content/File readiness" [data]="contentFile()" />
    </section>
  `
})
export class SriReadinessComponent {
  private readonly sriApi = inject(SriReadinessApiService);
  private readonly contentFileApi = inject(ContentFileReadinessApiService);
  protected readonly sri = signal<ReadinessResponse | null>(null);
  protected readonly contentFile = signal<ReadinessResponse | null>(null);
  protected readonly error = signal<string | null>(null);

  constructor() {
    this.sriApi.getReadiness().subscribe({ next: value => this.sri.set(value), error: error => this.error.set(error.message) });
    this.contentFileApi.getReadiness().subscribe({ next: value => this.contentFile.set(value), error: error => this.error.set(error.message) });
  }
}
