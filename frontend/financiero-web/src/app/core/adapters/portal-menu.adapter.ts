import { Injectable, computed, inject } from '@angular/core';
import { PortalContextAdapter } from '../portal-shell/portal-context.adapter';
import { PortalFeatureFlagAdapter } from './portal-feature-flag.adapter';

export interface PortalMenuItem {
  label: string;
  path: string;
  permission: string;
  icon?: string;
  order?: number;
  foundationOnly?: boolean;
  readOnly?: boolean;
}

@Injectable({ providedIn: 'root' })
export class PortalMenuAdapter {
  private readonly context = inject(PortalContextAdapter);
  private readonly flags = inject(PortalFeatureFlagAdapter);
  readonly items = computed<PortalMenuItem[]>(() => this.context.getContext().menu.items
    .filter(item => !item.featureFlag || this.flags.enabled(item.featureFlag))
    .sort((a, b) => (a.order ?? 999) - (b.order ?? 999))
    .map(item => ({
      label: item.title,
      path: item.route,
      permission: item.permission,
      icon: item.icon,
      order: item.order,
      foundationOnly: item.foundationOnly,
      readOnly: item.readOnly
    })));

  currentTitle(): string {
    return this.context.isPortalIntegrated() ? 'Financiero · Portal Shell' : 'Financiero · Standalone Shell';
  }
}
