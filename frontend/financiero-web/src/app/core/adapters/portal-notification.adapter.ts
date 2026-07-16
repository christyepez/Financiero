import { Injectable, inject } from '@angular/core';
import { PortalContextAdapter } from '../portal-shell/portal-context.adapter';

@Injectable({ providedIn: 'root' })
export class PortalNotificationAdapter {
  private readonly context = inject(PortalContextAdapter);

  foundationStatus(): string {
    return this.context.getContext().notifications.delegatedToPortal ? 'Portal notifications delegated' : 'Standalone notifications';
  }

  channels(): string[] {
    return this.context.getContext().notifications.channels;
  }
}
