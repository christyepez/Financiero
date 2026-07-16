import { InjectionToken } from '@angular/core';

export type PortalShellMode = 'standalone' | 'portal-integrated';
export type PortalShellSource = 'standalone' | 'portal';

export const SUPPORTED_PORTAL_SHELL_CONTRACT_VERSION = '1.0';

export interface PortalUserContext {
  userId: string;
  displayName: string;
  emailMasked?: string;
  roles?: string[];
}

export interface PortalTenantContext {
  tenantId: string;
  tenantName?: string;
}

export interface PortalPermissionContext {
  permissions: string[];
  roles?: string[];
}

export interface PortalFeatureFlags {
  showAtsXmlReadiness: boolean;
  showExternalApprovals: boolean;
  showPurchases: boolean;
  showVoidedDocuments: boolean;
  allowDevHeaders: boolean;
  allowPurchaseCommands: boolean;
  allowVoidedDocumentCommands: boolean;
  allowAtsOfficialActions: boolean;
  allowSriSubmission: boolean;
  allowXmlPreviewUi: boolean;
  allowMutations: boolean;
}

export interface PortalMenuItemContract {
  route: string;
  title: string;
  permission: string;
  featureFlag?: keyof PortalFeatureFlags;
  icon?: string;
  order?: number;
  foundationOnly?: boolean;
  readOnly?: boolean;
}

export interface PortalMenuContext {
  items: PortalMenuItemContract[];
}

export interface PortalNotificationContext {
  channels: string[];
  delegatedToPortal: boolean;
}

export interface PortalCorrelationContext {
  correlationId: string;
}

export interface PortalEnvironmentContext {
  apiBaseUrl: string;
  shellMode: PortalShellMode;
  production: boolean;
}

export interface PortalShellContext {
  contractVersion: string;
  source: PortalShellSource;
  issuedAt?: string;
  expiresAt?: string;
  capabilities: string[];
  warnings: string[];
  user: PortalUserContext;
  tenant: PortalTenantContext;
  permissions: PortalPermissionContext;
  menu: PortalMenuContext;
  notifications: PortalNotificationContext;
  correlation: PortalCorrelationContext;
  environment: PortalEnvironmentContext;
  featureFlags: PortalFeatureFlags;
  delegatedAuthToken?: string;
}

export interface PortalShellContextProvider {
  getContext(): PortalShellContext;
}

export const PORTAL_SHELL_CONTEXT = new InjectionToken<Partial<PortalShellContext> | null>('PORTAL_SHELL_CONTEXT', {
  providedIn: 'root',
  factory: () => null
});

declare global {
  interface Window {
    __PORTAL_SHELL_CONTEXT__?: Partial<PortalShellContext>;
  }
}
