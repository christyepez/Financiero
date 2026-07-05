# Implementation Backlog

## Epic 0 - Portal Discovery

| ID | Task | Agent | Output |
|---|---|---|---|
| F-000 | Inspect PortalCorporativo repository | Portal Capability Discovery | portal-capability-inventory.md |
| F-001 | Identify reusable APIs | Portal Capability Discovery | portal-reuse-matrix.md |
| F-002 | Identify do-not-duplicate components | Portal Reuse Architect | do-not-duplicate-list.md |
| F-003 | Define portal extension plan | Portal Reuse Architect | portal-extension-plan.md |

## Epic 1 - Portal Integration Foundation

| ID | Task | Agent | Output |
|---|---|---|---|
| F-100 | Create audit adapter contract | Audit Reuse | IPortalAuditPublisher |
| F-101 | Create notification adapter contract | Notification Reuse | IPortalNotificationPublisher |
| F-102 | Create security permission catalog | Security Extension | finance permissions |
| F-103 | Create menu registration contract | UI Portal Integration | finance menu seed |
| F-104 | Decide outbox reuse/extension | Transactional Bus | ADR |

## Epic 2 - Accounting Core

| ID | Task | Agent | Output |
|---|---|---|---|
| F-200 | Chart of accounts | Accounting | module contract |
| F-201 | Fiscal periods | Accounting | module contract |
| F-202 | Journal entries | Accounting | APIs/events |
| F-203 | Posting engine | Accounting | accounting rules |

## Epic 3 - Billing and Ecuador Tax

| ID | Task | Agent | Output |
|---|---|---|---|
| F-300 | Invoice lifecycle | Billing | APIs/events |
| F-301 | Credit note lifecycle | Billing | APIs/events |
| F-302 | Debit note lifecycle | Billing | APIs/events |
| F-303 | Withholding lifecycle | Tax Ecuador | APIs/events |
| F-304 | Tax rule engine | Tax Ecuador | rules/catalogs |

## Epic 4 - SRI and Electronic Documents

| ID | Task | Agent | Output |
|---|---|---|---|
| F-400 | Access key generator | Electronic Documents | service |
| F-401 | XML generator | Electronic Documents | service |
| F-402 | XSD validator | Electronic Documents | service |
| F-403 | Digital signature | Electronic Documents | service |
| F-404 | SRI reception client | SRI Integration | client |
| F-405 | SRI authorization client | SRI Integration | client |
| F-406 | RIDE generator | Electronic Documents | service |

## Epic 5 - Reports and Integrations

| ID | Task | Agent | Output |
|---|---|---|---|
| F-500 | ATS generator | Tax Ecuador | report/export |
| F-501 | Financial reports | Reporting | read models |
| F-502 | ERP adapters | Integration Adapters | SAP/Dynamics/Oracle/QuickBooks |
| F-503 | CRM adapters | Integration Adapters | Salesforce/Dynamics CRM |
