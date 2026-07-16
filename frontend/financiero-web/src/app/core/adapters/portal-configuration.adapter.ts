import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { PortalContextAdapter } from '../portal-shell/portal-context.adapter';
import { PortalFeatureFlags } from '../portal-shell/portal-shell.models';

@Injectable({ providedIn: 'root' })
export class PortalConfigurationAdapter {
  private readonly context = inject(PortalContextAdapter);

  apiBaseUrl(): string {
    return this.context.getContext().environment.apiBaseUrl || environment.apiBaseUrl;
  }

  featureFlags(): PortalFeatureFlags {
    return this.context.getContext().featureFlags;
  }
}
