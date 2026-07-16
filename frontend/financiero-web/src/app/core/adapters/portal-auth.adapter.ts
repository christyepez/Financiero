import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { PortalContextAdapter } from '../portal-shell/portal-context.adapter';

@Injectable({ providedIn: 'root' })
export class PortalAuthAdapter {
  private readonly context = inject(PortalContextAdapter);

  getAccessToken(): string | null {
    return this.context.getContext().delegatedAuthToken ?? null;
  }

  getDevelopmentPermissions(): string | null {
    const context = this.context.getContext();
    if (environment.production || context.environment.shellMode === 'portal-integrated') return null;
    return context.featureFlags.allowDevHeaders && environment.enableDevHeaders ? context.permissions.permissions.join(',') : null;
  }

  permissions(): string[] {
    return this.context.getContext().permissions.permissions;
  }
}
