import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { SanitizationService } from '../services/sanitization.service';
import { defaultFeatureFlags, defaultPortalCapabilities, financialMenuItems, standalonePortalShellContext } from './portal-shell.defaults';
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
    const localEmbeddedContext = portalLocalEmbeddedContext();
    return sanitizePortalShellContext(this.injectedContext || windowContext || localEmbeddedContext, this.sanitizer);
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

  hasUnsupportedContract(): boolean {
    const context = this.getContext();
    return context.contractVersion !== '1.0';
  }

  contextWarnings(): string[] {
    return this.getContext().warnings;
  }
}


function portalLocalEmbeddedContext(): Partial<PortalShellContext> | null {
  if (typeof window === 'undefined' || !window.location.pathname.startsWith('/apps/financial/')) return null;

  const now = new Date();
  return {
    contractVersion: '1.0',
    source: 'portal',
    issuedAt: now.toISOString(),
    expiresAt: new Date(now.getTime() + 10 * 60 * 1000).toISOString(),
    capabilities: defaultPortalCapabilities,
    warnings: ['Portal PROD-local embedded SuperAdmin context.'],
    user: {
      userId: 'portal-local-superadmin',
      displayName: 'Local SuperAdmin',
      emailMasked: 's********n@portal.local',
      roles: ['SuperAdmin']
    },
    tenant: { tenantId: 'default', tenantName: 'Default Tenant' },
    permissions: {
      permissions: ['financial.*', ...new Set(financialMenuItems.map(item => item.permission))],
      roles: ['SuperAdmin']
    },
    menu: { items: financialMenuItems },
    notifications: { channels: ['portal'], delegatedToPortal: true },
    correlation: { correlationId: globalThis.crypto?.randomUUID?.() ?? `financial-portal-${Date.now()}` },
    environment: { apiBaseUrl: '/api/financial', shellMode: 'portal-integrated', production: true },
    featureFlags: { ...defaultFeatureFlags }
  };
}
