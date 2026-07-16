import { environment } from '../../../environments/environment';
import { PortalFeatureFlags, PortalMenuItemContract, PortalShellContext } from './portal-shell.models';

export const defaultFeatureFlags: PortalFeatureFlags = {
  showAtsXmlReadiness: true,
  showExternalApprovals: true,
  showPurchases: true,
  showVoidedDocuments: true,
  allowDevHeaders: false,
  allowXmlPreviewUi: false,
  allowMutations: false
};

export const financialMenuItems: PortalMenuItemContract[] = [
  { route: '/dashboard', title: 'Dashboard', permission: 'financial.electronicdocuments.read', icon: 'dashboard', order: 10, foundationOnly: true, readOnly: true },
  { route: '/sri-readiness', title: 'SRI readiness', permission: 'financial.electronicdocuments.manage', icon: 'shield', order: 20, foundationOnly: true, readOnly: true },
  { route: '/ats-readiness', title: 'ATS readiness', permission: 'financial.electronicdocuments.read', featureFlag: 'showAtsXmlReadiness', icon: 'report', order: 30, foundationOnly: true, readOnly: true },
  { route: '/external-approvals', title: 'Aprobaciones externas', permission: 'financial.electronicdocuments.manage', featureFlag: 'showExternalApprovals', icon: 'approval', order: 40, foundationOnly: true, readOnly: true },
  { route: '/tax-catalogs', title: 'Catálogos tributarios', permission: 'financial.electronicdocuments.read', icon: 'catalog', order: 50, foundationOnly: true, readOnly: true },
  { route: '/purchases', title: 'Compras', permission: 'financial.electronicdocuments.read', featureFlag: 'showPurchases', icon: 'purchase', order: 60, foundationOnly: true, readOnly: true },
  { route: '/voided-documents', title: 'Anulados', permission: 'financial.electronicdocuments.read', featureFlag: 'showVoidedDocuments', icon: 'voided', order: 70, foundationOnly: true, readOnly: true }
];

export function standalonePortalShellContext(): PortalShellContext {
  const allowDevHeaders = !environment.production && environment.enableDevHeaders;
  const permissions = allowDevHeaders
    ? environment.devPermissionsHeader.split(',').map(x => x.trim()).filter(Boolean)
    : [];

  return {
    user: { userId: 'standalone-dev-user', displayName: 'Standalone foundation user', emailMasked: 's********e@example.local', roles: [] },
    tenant: { tenantId: 'default', tenantName: 'Standalone tenant' },
    permissions: { permissions, roles: [] },
    menu: { items: financialMenuItems },
    notifications: { channels: ['local-banner'], delegatedToPortal: false },
    correlation: { correlationId: globalThis.crypto?.randomUUID?.() ?? `fin-${Date.now()}` },
    environment: { apiBaseUrl: environment.apiBaseUrl, shellMode: 'standalone', production: environment.production },
    featureFlags: { ...defaultFeatureFlags, allowDevHeaders }
  };
}
