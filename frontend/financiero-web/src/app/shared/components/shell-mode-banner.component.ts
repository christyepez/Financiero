import { Component, inject } from '@angular/core';
import { PortalContextAdapter } from '../../core/portal-shell/portal-context.adapter';
import { FeatureFlagBadgeComponent } from './feature-flag-badge.component';
import { PermissionHintComponent } from './permission-hint.component';

@Component({
  selector: 'fin-shell-mode-banner',
  standalone: true,
  imports: [FeatureFlagBadgeComponent, PermissionHintComponent],
  template: `
    <div class="warning">
      <strong>{{ modeLabel }}</strong>
      · tenant {{ tenant }}
      · {{ productionLabel }}
      <fin-permission-hint [permissions]="permissions" />
      <fin-feature-flag-badge />
      @if (missingPortalContext) {
        <div>Portal context is required in production. No delegated login is created by Financiero.</div>
      }
    </div>
  `
})
export class ShellModeBannerComponent {
  private readonly context = inject(PortalContextAdapter);
  protected readonly value = this.context.getContext();
  protected readonly modeLabel = this.value.environment.shellMode === 'portal-integrated' ? 'Portal integrated foundation mode' : 'Standalone foundation mode';
  protected readonly tenant = this.value.tenant.tenantName || this.value.tenant.tenantId;
  protected readonly productionLabel = this.value.environment.production ? 'production-safe contract' : 'development-safe contract';
  protected readonly permissions = this.value.permissions.permissions;
  protected readonly missingPortalContext = this.context.missingRequiredPortalContext();
}
