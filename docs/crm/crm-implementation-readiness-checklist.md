# CRM Implementation Readiness Checklist

Date: 2026-07-20
Status: GO/NO-GO checklist

## Preconditions

| Precondition | Evidence required | Current state |
|---|---|---|
| Executive decision | Approved CRM implementation path. | Pending |
| Repo/module defined | Decision: separate repo, Portal module, or platform bounded context. | Pending |
| Portal Auth/Menu/permissions available | Accepted live Portal evidence. | Pending |
| SQL common or DB strategy | Shared SQL/DB strategy accepted; no SQL propio. | Pending |
| Financiero PASS or decoupling | PASS E2E real or formal decoupling decision. | Pending |
| Master data architecture | Customer/account ownership decided. | Pending |
| Security rules | Roles, permissions, PII, masking and audit rules approved. | Pending |
| Tenant model | Single/multi-tenant decision approved. | Pending |
| Audit | Portal Audit reuse path approved. | Pending |
| Notifications | Portal Notification reuse path approved. | Pending |
| Reporting | Reporting/BI boundary approved. | Pending |

## GO criteria

- All critical preconditions are approved.
- CRM target architecture does not duplicate Portal capabilities.
- CRM data ownership does not conflict with Financiero or future Master Data.
- Product Owner accepts implementation scope and non-production guardrails.

## NO-GO criteria

- Portal/SQL base remains unresolved with no decoupling decision.
- CRM would require its own Auth/Menu/Gateway/SQL to proceed.
- Customer identity ownership is unclear.
- Implementation would add CRM runtime in Financiero without architecture approval.
- Evidence contains secrets, real personal data, XML reales or production SRI data.

## Risks

- CRM implemented on unstable platform base.
- Customer/master data duplication.
- Financial and commercial ownership blur.
- Discovery mistaken for implementation approval.

## Evidence required

- Architecture decision.
- Product Owner approval.
- Security/tenant model approval.
- Portal capability readiness or explicit decoupling.
- Database strategy approval.
- Master data decision.
