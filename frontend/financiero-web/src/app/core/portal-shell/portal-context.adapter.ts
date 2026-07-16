import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { SanitizationService } from '../services/sanitization.service';
import { standalonePortalShellContext } from './portal-shell.defaults';
import { PORTAL_SHELL_CONTEXT, PortalShellContext, PortalShellContextProvider } from './portal-shell.models';
import { sanitizePortalShellContext } from './portal-shell-validation';

@Injectable({ providedIn: 'root' })
export class StandalonePortalContextProvider implements PortalShellContextProvider {
  getContext(): PortalShellContext {
    return standalonePortalShellContext();
  }
}

@Injectable({ providedIn: 'root' })
export class PortalIntegratedContextProvider implements PortalShellContextProvider {
  private readonly injectedContext = inject(PORTAL_SHELL_CONTEXT);
  private readonly sanitizer = inject(SanitizationService);

  getContext(): PortalShellContext {
    const windowContext = typeof window !== 'undefined' ? window.__PORTAL_SHELL_CONTEXT__ : null;
    return sanitizePortalShellContext(this.injectedContext || windowContext, this.sanitizer);
  }
}

@Injectable({ providedIn: 'root' })
export class PortalContextAdapter {
  private readonly standalone = inject(StandalonePortalContextProvider);
  private readonly integrated = inject(PortalIntegratedContextProvider);

  getContext(): PortalShellContext {
    const integratedContext = this.integrated.getContext();
    if (integratedContext.environment.shellMode === 'portal-integrated') return integratedContext;
    if (environment.production) return integratedContext;
    return this.standalone.getContext();
  }

  isPortalIntegrated(): boolean {
    return this.getContext().environment.shellMode === 'portal-integrated';
  }

  missingRequiredPortalContext(): boolean {
    const context = this.getContext();
    return environment.production && context.environment.shellMode === 'portal-integrated' && context.user.userId === 'missing-portal-context';
  }
}
