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
