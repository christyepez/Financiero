import { Injectable, inject } from '@angular/core';
import { PortalContextAdapter } from '../portal-shell/portal-context.adapter';

@Injectable({ providedIn: 'root' })
export class PortalTelemetryAdapter {
  private readonly context = inject(PortalContextAdapter);

  correlationId(): string {
    return this.context.getContext().correlation.correlationId;
  }

  track(eventName: 'shell_context_loaded' | 'shell_context_invalid' | 'menu_resolved' | 'command_blocked' | 'command_attempted_foundation', metadata: Record<string, unknown> = {}): void {
    void eventName;
    void this.sanitizeMetadata(metadata);
  }

  private sanitizeMetadata(metadata: Record<string, unknown>): Record<string, string> {
    return Object.fromEntries(Object.entries(metadata).map(([key, value]) => [key, String(value ?? '').replace(/(token|secret|password|email|identification|accessKey)[^,;\s]*/gi, '[redacted]')]));
  }
}
