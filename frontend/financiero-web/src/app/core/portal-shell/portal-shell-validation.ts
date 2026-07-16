import { environment } from '../../../environments/environment';
import { SanitizationService } from '../services/sanitization.service';
import { allowedFinancialRoutes, defaultFeatureFlags, defaultPortalCapabilities, financialMenuItems, standalonePortalShellContext } from './portal-shell.defaults';
import { PortalMenuItemContract, PortalShellContext, SUPPORTED_PORTAL_SHELL_CONTRACT_VERSION } from './portal-shell.models';

export function sanitizePortalShellContext(input: Partial<PortalShellContext> | null | undefined, sanitizer: SanitizationService): PortalShellContext {
  if (!input) {
    if (environment.production) {
      return {
        ...standalonePortalShellContext(),
        source: 'portal',
        warnings: ['Portal context is required in production.'],
        user: { userId: 'missing-portal-context', displayName: 'Portal context required' },
        permissions: { permissions: [] },
        environment: { apiBaseUrl: environment.apiBaseUrl, shellMode: 'portal-integrated', production: true },
        featureFlags: { ...defaultFeatureFlags, allowDevHeaders: false, allowMutations: false, allowXmlPreviewUi: false }
      };
    }

    return standalonePortalShellContext();
  }

  const standalone = standalonePortalShellContext();
  const warnings: string[] = sanitizeList(input.warnings, sanitizer);
  const shellMode = input.environment?.shellMode === 'portal-integrated' ? 'portal-integrated' : 'standalone';
  const portalIntegrated = shellMode === 'portal-integrated';
  const contractVersion = sanitizeContractVersion(input.contractVersion, warnings);
  if (portalIntegrated) {
    if (!input.user?.userId) warnings.push('Portal integrated context missing required user.');
    if (!input.tenant?.tenantId) warnings.push('Portal integrated context missing required tenant.');
    if (!input.permissions?.permissions?.length) warnings.push('Portal integrated context missing permissions.');
  }
  const featureFlags = {
    ...defaultFeatureFlags,
    ...(input.featureFlags ?? {}),
    allowDevHeaders: !environment.production && Boolean(input.featureFlags?.allowDevHeaders),
    allowMutations: !environment.production && Boolean(input.featureFlags?.allowMutations),
    allowPurchaseCommands: !environment.production && Boolean(input.featureFlags?.allowPurchaseCommands),
    allowVoidedDocumentCommands: !environment.production && Boolean(input.featureFlags?.allowVoidedDocumentCommands),
    allowAtsOfficialActions: false,
    allowSriSubmission: false,
    allowXmlPreviewUi: false
  };

  return {
    contractVersion,
    source: portalIntegrated ? 'portal' : 'standalone',
    issuedAt: sanitizeIsoDate(input.issuedAt, sanitizer),
    expiresAt: sanitizeIsoDate(input.expiresAt, sanitizer),
    capabilities: sanitizeList(input.capabilities, sanitizer).length ? sanitizeList(input.capabilities, sanitizer) : defaultPortalCapabilities,
    warnings,
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
      items: sanitizeMenuItems(input.menu?.items?.length ? input.menu.items : financialMenuItems, sanitizer, warnings, portalIntegrated).map(item => ({
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
      delegatedToPortal: portalIntegrated && Boolean(input.notifications?.delegatedToPortal)
    },
    correlation: { correlationId: sanitizer.safeText(input.correlation?.correlationId || standalone.correlation.correlationId) },
    environment: {
      apiBaseUrl: sanitizer.safeText(input.environment?.apiBaseUrl || environment.apiBaseUrl),
      shellMode,
      production: environment.production
    },
    featureFlags,
    delegatedAuthToken: portalIntegrated ? sanitizeDelegatedToken(input.delegatedAuthToken, warnings) : undefined
  };
}

function sanitizeList(values: string[] | undefined, sanitizer: SanitizationService): string[] {
  return (values ?? []).map(value => sanitizer.safeText(value)).filter(Boolean);
}

function sanitizeContractVersion(value: string | undefined, warnings: string[]): string {
  if (value === SUPPORTED_PORTAL_SHELL_CONTRACT_VERSION) return value;
  warnings.push(`Unsupported Portal Shell contract version. Expected ${SUPPORTED_PORTAL_SHELL_CONTRACT_VERSION}.`);
  return value || 'unsupported';
}

function sanitizeIsoDate(value: string | undefined, sanitizer: SanitizationService): string | undefined {
  if (!value) return undefined;
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? undefined : sanitizer.safeText(date.toISOString());
}

function sanitizeMenuItems(items: PortalMenuItemContract[], sanitizer: SanitizationService, warnings: string[], portalIntegrated: boolean): PortalMenuItemContract[] {
  const filtered = items.filter(item => {
    const route = sanitizer.safeText(item.route || '');
    const allowed = allowedFinancialRoutes.includes(route);
    if (!allowed) warnings.push('Portal menu route rejected by Financiero allow-list.');
    return allowed;
  });
  if (!filtered.length && !portalIntegrated) return financialMenuItems;
  return filtered;
}

function sanitizeDelegatedToken(value: string | undefined, warnings: string[]): string | undefined {
  if (!value) return undefined;
  if (/[\s?&=]/.test(value)) {
    warnings.push('Delegated auth token rejected because it does not look like an in-memory bearer value.');
    return undefined;
  }
  return value;
}
