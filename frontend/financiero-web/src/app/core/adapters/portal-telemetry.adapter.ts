import { Injectable, inject } from '@angular/core';
import { PortalContextAdapter } from '../portal-shell/portal-context.adapter';

@Injectable({ providedIn: 'root' })
export class PortalTelemetryAdapter {
  private readonly context = inject(PortalContextAdapter);

  correlationId(): string {
    return this.context.getContext().correlation.correlationId;
  }
}
