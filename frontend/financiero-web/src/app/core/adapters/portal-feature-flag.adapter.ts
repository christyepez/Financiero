import { Injectable, inject } from '@angular/core';
import { PortalFeatureFlags } from '../portal-shell/portal-shell.models';
import { PortalConfigurationAdapter } from './portal-configuration.adapter';

@Injectable({ providedIn: 'root' })
export class PortalFeatureFlagAdapter {
  private readonly configuration = inject(PortalConfigurationAdapter);

  enabled(flag: keyof PortalFeatureFlags): boolean {
    return Boolean(this.configuration.featureFlags()[flag]);
  }
}
