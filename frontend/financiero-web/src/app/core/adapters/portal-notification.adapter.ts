import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class PortalNotificationAdapter {
  foundationStatus(): string {
    return 'Portal integrated';
  }
}
