# CRM Bounded Context

Date: 2026-07-20
Status: conceptual; no runtime

## Bounded context

CRM owns the commercial relationship lifecycle before, during and after conversion into a customer. It should model prospects, accounts, contacts, opportunities, activities and relationship history without owning financial accounting or tax operations.

## CRM responsibilities

- Lead/prospect capture and qualification.
- Account/company relationship management.
- Contact management.
- Opportunity and pipeline tracking.
- Sales activities, tasks, notes and interactions.
- Campaign and segment concepts.
- Conversion handoff to customer/master data.
- Commercial follow-up and future customer health signals.

## Not CRM responsibilities

- Accounting ledger, journal entries or fiscal periods.
- Electronic invoicing, SRI XML, authorizations, ATS or RIDE.
- Collections ledger or tax reporting.
- Authentication, identity, roles, menu, permissions or tenant ownership.
- Portal audit, notification, configuration, content/file or gateway implementation.
- Master customer identity if a future Master Data context owns it.

## Relationship with Financiero

CRM may reference future financial customer state, invoices, receivables, payment status and account statements. Financiero should remain the owner of financial documents, accounting and tax facts. CRM should consume financial summaries or events, not write financial truth.

## Relationship with Portal

CRM must reuse Portal capabilities for Auth/Menu/permissions, Gateway, Audit, Notification, Configuration, Content/File and observability. CRM must not duplicate Portal-owned platform services.

## Relationship with future modules

| Module | Relationship |
|---|---|
| Ventas | CRM produces qualified opportunities and commercial context. |
| Cobranzas | CRM may consume overdue/payment signals and schedule follow-up. |
| Soporte | CRM may consume support history for customer context. |
| Marketing | CRM may exchange campaigns, segments and lead sources. |
| Cartera | CRM may show portfolio/customer health, but not own financial balances. |

## Architecture decisions

- CRM is not embedded inside billing or SRI flows.
- CRM must not duplicate financial customers if a future Master Data context owns customer identity.
- CRM must not duplicate Auth/Menu/permissions.
- CRM implementation remains blocked until platform readiness or formal decoupling is approved.
