import { Component, inject } from '@angular/core';
import { PortalConfigurationAdapter } from '../../core/adapters/portal-configuration.adapter';
import { StatusBadgeComponent } from './status-badge.component';

@Component({
  selector: 'fin-feature-flag-badge',
  standalone: true,
  imports: [StatusBadgeComponent],
  template: `<fin-status-badge [value]="label" />`
})
export class FeatureFlagBadgeComponent {
  private readonly flags = inject(PortalConfigurationAdapter).featureFlags();
  protected readonly label = this.flags.allowMutations || this.flags.allowXmlPreviewUi ? 'Unsafe flags blocked' : 'Safe feature flags';
}
