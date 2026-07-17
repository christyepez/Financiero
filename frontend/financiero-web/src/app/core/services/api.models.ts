export interface ReadinessIssue {
  code?: string;
  message?: string;
  severity?: string;
  field?: string;
  notes?: string;
  value?: string;
}

export interface ReadinessResponse {
  period?: string;
  status?: string;
  isReady?: boolean;
  ready?: boolean;
  disclaimer?: string;
  provider?: string;
  mode?: string;
  catalogVersion?: string;
  issues?: ReadinessIssue[];
  warnings?: ReadinessIssue[];
  checks?: string[];
  blockedReasons?: ReadinessIssue[];
  requiredExternalApprovals?: string[];
  missingRequirements?: string[];
  requiredEvidence?: string[];
  blockingRisks?: string[];
  recommendedNextAction?: string;
  gates?: ExternalApprovalGate[];
  sections?: AtsSectionReadiness[];
  [key: string]: unknown;
}

export interface CatalogItem {
  code?: string;
  name?: string;
  description?: string;
  version?: string;
  active?: boolean;
  requiresTaxReview?: boolean;
  isFoundationOnly?: boolean;
  requiresAuthorization?: boolean;
  requiresAccessKey?: boolean;
  appliesToAts?: boolean;
  [key: string]: unknown;
}

export interface TaxCatalogSummary {
  version?: string;
  disclaimer?: string;
  purchaseDocumentTypes?: CatalogItem[];
  supportDocumentTypes?: CatalogItem[];
  voidedDocumentTypes?: CatalogItem[];
  purchaseTaxCodes?: CatalogItem[];
  supplierIdentificationTypes?: CatalogItem[];
}

export interface TaxDocumentSummary {
  id?: string;
  period?: string;
  fiscalPeriod?: string;
  documentType?: string;
  documentNumber?: string;
  supplierName?: string;
  supplierIdentificationMasked?: string;
  establishment?: string;
  emissionPoint?: string;
  sequential?: string;
  reason?: string;
  status?: string;
  total?: number;
  subtotal?: number;
  taxTotal?: number;
  accessKeyMasked?: string;
  authorizationNumberMasked?: string;
  disclaimer?: string;
  [key: string]: unknown;
}

export interface ProductizationReadinessResult {
  scope?: string;
  documentId?: string;
  status?: string;
  isReadyForProduction?: boolean;
  doesNotEnableProduction?: boolean;
  gates?: { code?: string; status?: string; description?: string; required?: boolean; isFoundationOnly?: boolean }[];
  blockers?: { code?: string; message?: string; severity?: string }[];
  recommendations?: { code?: string; message?: string }[];
  portalBoundaries?: unknown;
  dangerousFeatureFlagsBlocked?: string[];
  disclaimer?: string;
}

export interface CreatePurchaseTaxDocumentRequest {
  supplierIdentificationType: string;
  supplierIdentification: string;
  supplierName: string;
  establishment: string;
  emissionPoint: string;
  sequential: string;
  documentType: string;
  issueDate: string;
  registrationDate: string;
  fiscalPeriod: string;
  supportDocumentType: string;
  subtotal: number;
  taxTotal: number;
  total: number;
  currency: string;
  authorizationNumber?: string;
  accessKey?: string;
}

export interface AddPurchaseLineRequest {
  productCode: string;
  description: string;
  quantity: number;
  unitPrice: number;
  discount: number;
}

export interface AddPurchaseTaxRequest {
  taxCode: string;
  taxPercentageCode: string;
  taxBase: number;
  taxRate: number;
  taxAmount: number;
}

export interface RegisterVoidedTaxDocumentRequest {
  documentType: string;
  establishment: string;
  emissionPoint: string;
  sequential: string;
  issueDate: string;
  voidDate: string;
  fiscalPeriod: string;
  reason: string;
  authorizationNumber?: string;
  accessKey?: string;
}

export interface AtsSectionReadiness {
  section?: string;
  mappedCount?: number;
  missingCount?: number;
  unsupportedCount?: number;
  requiredFields?: string[];
  missingFields?: string[];
  warnings?: string[];
  disclaimer?: string;
}

export interface ExternalApprovalGate {
  scope?: string;
  status?: string;
  required?: boolean;
  approved?: boolean;
  message?: string;
  isFoundationOnly?: boolean;
  requiresHumanReview?: boolean;
  requiresEvidence?: boolean;
  requirements?: { code?: string; description?: string; requiresEvidence?: boolean; requiresHumanReview?: boolean }[];
  blockingRisks?: string[];
  recommendedNextAction?: string;
  disclaimer?: string;
  [key: string]: unknown;
}

export interface ExternalApprovalRequestSummary {
  id: string;
  scope: string;
  status: string;
  title: string;
  fiscalPeriod?: string;
  createdAtUtc: string;
  updatedAtUtc: string;
  doesNotEnableProduction: boolean;
  requirements?: { description: string; requiresEvidence: boolean; requiresHumanReview: boolean }[];
  evidenceReferences?: { id: string; provider: string; referenceId: string; displayName: string; hash?: string; contentType?: string; createdAtUtc: string; createdByDisplayName?: string }[];
  decisions?: { id: string; decisionKind: string; reason: string; decidedByDisplayName: string; decidedAtUtc: string; doesNotEnableProduction: boolean }[];
  disclaimer?: string;
}

export interface CreateExternalApprovalRequest {
  scope: string;
  title: string;
  fiscalPeriod?: string;
  createdByDisplayName: string;
  requirements: { description: string; requiresEvidence: boolean; requiresHumanReview: boolean }[];
}

export interface AddExternalApprovalEvidenceReferenceRequest {
  provider: string;
  referenceId: string;
  displayName: string;
  hash?: string;
  contentType?: string;
  createdByDisplayName?: string;
  sizeBytes?: number;
  purpose?: string;
  retentionHint?: string;
}

export interface ExternalApprovalIntegrationReadiness {
  contentFile?: { status?: string; isPortalOwned?: boolean; allowsUpload?: boolean; allowsDownload?: boolean; allowsSensitiveLinks?: boolean; missingPortalDependencies?: string[]; blockers?: string[]; disclaimer?: string };
  notification?: { status?: string; isPortalOwned?: boolean; allowsSend?: boolean; missingPortalDependencies?: string[]; blockers?: string[]; disclaimer?: string };
  auditOutboxStatus?: string;
  featureFlags?: string[];
  missingPortalDependencies?: string[];
  blockers?: string[];
  recommendedNextAction?: string;
  disclaimer?: string;
}

export interface RecordExternalApprovalDecisionRequest {
  decision: 'ApprovedFoundation' | 'RejectedFoundation' | 'Blocked';
  reason: string;
  decidedByDisplayName: string;
}

export interface ApiEnvelope<T> {
  data?: T | null;
  error?: { code?: string; message?: string } | null;
  correlationId?: string;
}

export interface ViewState<T> {
  loading: boolean;
  data: T;
  error: string | null;
  updatedAt: string | null;
}
