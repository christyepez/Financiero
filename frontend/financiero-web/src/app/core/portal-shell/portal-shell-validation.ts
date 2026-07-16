import { environment } from '../../../environments/environment';
import { SanitizationService } from '../services/sanitization.service';
import { defaultFeatureFlags, financialMenuItems, standalonePortalShellContext } from './portal-shell.defaults';
import { PortalShellContext } from './portal-shell.models';

export function sanitizePortalShellContext(input: Partial<PortalShellContext> | null | undefined, sanitizer: SanitizationService): PortalShellContext {
  if (!input) {
    if (environment.production) {
      return {
        ...standalonePortalShellContext(),
        user: { userId: 'missing-portal-context', displayName: 'Portal context required' },
        permissions: { permissions: [] },
        environment: { apiBaseUrl: environment.apiBaseUrl, shellMode: 'portal-integrated', production: true },
        featureFlags: { ...defaultFeatureFlags, allowDevHeaders: false, allowMutations: false, allowXmlPreviewUi: false }
      };
    }

    return standalonePortalShellContext();
  }

  const standalone = standalonePortalShellContext();
  const featureFlags = {
    ...defaultFeatureFlags,
    ...(input.featureFlags ?? {}),
    allowDevHeaders: !environment.production && Boolean(input.featureFlags?.allowDevHeaders),
    allowMutations: false,
    allowXmlPreviewUi: false
  };

  return {
    user: {
      userId: sanitizer.safeText(input.user?.userId || standalone.user.userId),
      displayName: sanitizer.safeText(input.user?.displayName || standalone.user.displayName),
      emailMasked: input.user?.emailMasked ? sanitizer.maskEmail(input.user.emailMasked) : undefined,
      roles: sanitizeList(input.user?.roles, sanitizer)
    },
    tenant: {
      tenantId: sanitizer.safeText(input.tenant?.tenantId || standalone.tenant.tenantId),
      tenantName: input.tenant?.tenantName ? sanitizer.safeText(input.tenant.tenantName) : standalone.tenant.tenantName
    },
    permissions: {
      permissions: sanitizeList(input.permissions?.permissions, sanitizer),
      roles: sanitizeList(input.permissions?.roles, sanitizer)
    },
    menu: {
      items: (input.menu?.items?.length ? input.menu.items : financialMenuItems).map(item => ({
        route: sanitizer.safeText(item.route),
        title: sanitizer.safeText(item.title),
        permission: sanitizer.safeText(item.permission),
        featureFlag: item.featureFlag,
        icon: item.icon ? sanitizer.safeText(item.icon) : undefined,
        order: item.order,
        foundationOnly: item.foundationOnly ?? true,
        readOnly: item.readOnly ?? true
      }))
    },
    notifications: {
      channels: sanitizeList(input.notifications?.channels, sanitizer),
      delegatedToPortal: Boolean(input.notifications?.delegatedToPortal)
    },
    correlation: { correlationId: sanitizer.safeText(input.correlation?.correlationId || standalone.correlation.correlationId) },
    environment: {
      apiBaseUrl: sanitizer.safeText(input.environment?.apiBaseUrl || environment.apiBaseUrl),
      shellMode: input.environment?.shellMode === 'portal-integrated' ? 'portal-integrated' : 'standalone',
      production: environment.production
    },
    featureFlags,
    delegatedAuthToken: input.delegatedAuthToken
  };
}

function sanitizeList(values: string[] | undefined, sanitizer: SanitizationService): string[] {
  return (values ?? []).map(value => sanitizer.safeText(value)).filter(Boolean);
}
