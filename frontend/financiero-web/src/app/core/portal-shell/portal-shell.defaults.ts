import { environment } from '../../../environments/environment';
import { PortalFeatureFlags, PortalMenuItemContract, PortalShellContext, SUPPORTED_PORTAL_SHELL_CONTRACT_VERSION } from './portal-shell.models';

export const defaultFeatureFlags: PortalFeatureFlags = {
  showAtsXmlReadiness: true,
  showExternalApprovals: true,
  showPurchases: true,
  showVoidedDocuments: true,
  allowDevHeaders: false,
  allowPurchaseCommands: false,
  allowVoidedDocumentCommands: false,
  allowExternalApprovalCommands: false,
  allowEvidenceReferenceMetadata: false,
  allowApprovalDecisionFoundation: false,
  allowAtsOfficialActions: false,
  allowSriSubmission: false,
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

export const allowedFinancialRoutes = financialMenuItems.map(item => item.route);

export const defaultPortalCapabilities = [
  'financial.shell.foundation',
  'financial.readiness.read',
  'financial.commands.gated'
];

export function standalonePortalShellContext(): PortalShellContext {
  const allowDevHeaders = !environment.production && environment.enableDevHeaders;
  const permissions = ['financial.electronicdocuments.read'];
  if (allowDevHeaders) {
    permissions.push(...environment.devPermissionsHeader.split(',').map(x => x.trim()).filter(Boolean));
  }

  return {
    contractVersion: SUPPORTED_PORTAL_SHELL_CONTRACT_VERSION,
    source: 'standalone',
    issuedAt: new Date().toISOString(),
    capabilities: defaultPortalCapabilities,
    warnings: environment.production ? ['Standalone shell is not allowed in production.'] : ['Standalone development fallback.'],
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
