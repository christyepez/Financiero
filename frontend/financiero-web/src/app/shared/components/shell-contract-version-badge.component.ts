import { Component, inject } from '@angular/core';
import { PortalContextAdapter } from '../../core/portal-shell/portal-context.adapter';

@Component({
  selector: 'fin-shell-contract-version-badge',
  standalone: true,
  template: `<span class="badge info">Portal contract {{ version }}</span>`
})
export class ShellContractVersionBadgeComponent {
  private readonly context = inject(PortalContextAdapter);
  protected readonly version = this.context.getContext().contractVersion;
}
