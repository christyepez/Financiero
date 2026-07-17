# Sprint 7 Capability Matrix

| Capability | Estado | Owner | Reuse classification | Implementado en Financiero | Owner Portal | Riesgo | Próximo paso |
|---|---|---|---|---|---|---|---|
| Portal Shell Contract | Foundation ready | Portal + Financiero | EXTEND | Contract validation, warnings, fallback. | Portal Shell | Contract drift | Validate E2E with Portal. |
| Security/Auth | Delegated | Portal | REUSE | Permission checks only. | Security API | Missing real context | Validate Portal auth context. |
| Menu | Delegated metadata | Portal | EXTEND | Allow-list and flags. | Menu API | Route mismatch | Publish/consume real menu metadata. |
| Configuration | Delegated flags | Portal | EXTEND | Safe local flags. | Configuration API | Unsafe flag override | Centralize with Portal config. |
| Audit | Adapter usage | Portal | ADAPT | Existing audit/outbox path. | Audit API | Missing production audit contract | Validate contract. |
| Outbox | Adapter usage | Portal | ADAPT | Existing outbox path. | Outbox/Workers | Duplicate events | Validate idempotency. |
| Content/File | Boundary ready | Portal | ADAPT | Metadata/reference readiness only. | Content/File API | No real upload | Sprint 8 contract test. |
| Notification | Boundary ready | Portal | ADAPT | Intents/readiness only. | Notification API | No real send | Sprint 8 contract test. |
| External Approval Workflow | Foundation | Financiero | CREATE + ADAPT | Requests, decisions, sanitized references. | Audit/Content/File | Misread as legal approval | Keep foundation disclaimers. |
| Purchase Readiness | Foundation | Financiero | CREATE | Read-only gates/blockers. | Security/Audit | False production confidence | Add Portal E2E evidence. |
| Voided Document Readiness | Foundation | Financiero | CREATE | Read-only gates/blockers. | Security/Audit | Legal workflow incomplete | Legal review. |
| Productization Readiness | Foundation | Financiero | CREATE | Read-only blockers. | Portal capabilities | Activation pressure | Keep hard block. |
| SRI Test | Mock/dry-run | Financiero | BLOCKED | No real send. | Configuration/Secrets | Accidental external call | Manual approval only. |
| SRI Production | Blocked | External/SRI | BLOCKED | Not implemented. | Configuration/Secrets | Regulatory exposure | Dedicated production gate. |
| ATS Official | Blocked | Tax/Legal | BLOCKED | Foundation preview only. | Reporting/Content/File | Incorrect filing | Tax review and schema evidence. |
| RIDE Legal Final | Blocked | Tax/Legal | BLOCKED | Foundation layout only. | Content/File | Legal invalidity | Legal review. |
| XAdES Productive | Blocked | Security | BLOCKED | Placeholder/foundation only. | Secret Store | Certificate leakage | Custody design and approval. |
| Certificate Custody | Blocked | Security/DevOps | BLOCKED | No cert storage. | Secret Store | Secret exposure | External vault runbook. |

## Sprint 8 P1 extension

`/api/financial/portal-integration/readiness`, the Portal E2E checklist and the local runbook validate these capabilities as integration contracts. They do not change ownership: Portal remains owner of Security/Auth, Menu, Configuration, Audit, Outbox, Content/File, Notification and Shell context.
