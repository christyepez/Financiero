export interface ReadinessIssue {
  code?: string;
  message?: string;
  severity?: string;
  field?: string;
}

export interface ReadinessResponse {
  status?: string;
  isReady?: boolean;
  ready?: boolean;
  disclaimer?: string;
  provider?: string;
  mode?: string;
  issues?: ReadinessIssue[];
  warnings?: ReadinessIssue[];
  [key: string]: unknown;
}

export interface CatalogItem {
  code?: string;
  name?: string;
  description?: string;
  version?: string;
  active?: boolean;
  [key: string]: unknown;
}

export interface TaxDocumentSummary {
  id?: string;
  period?: string;
  documentType?: string;
  supplierName?: string;
  establishment?: string;
  emissionPoint?: string;
  sequential?: string;
  status?: string;
  total?: number;
  disclaimer?: string;
  [key: string]: unknown;
}

export interface ExternalApprovalGate {
  scope?: string;
  status?: string;
  required?: boolean;
  approved?: boolean;
  message?: string;
  [key: string]: unknown;
}
