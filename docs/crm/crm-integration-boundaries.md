# CRM Integration Boundaries

Date: 2026-07-20
Status: conceptual only

## Boundary matrix

| Boundary | Future integration style | Notes |
|---|---|---|
| CRM ↔ Portal | Reuse Portal Auth/Menu/permissions, Gateway, Audit, Notification, Configuration and observability. | CRM must not implement platform capabilities. |
| CRM ↔ Financiero | Events and read-only API summaries. | Financiero owns invoices, receivables, accounting and tax facts. |
| CRM ↔ Facturación | Events such as `InvoiceCreated`; optional read API later. | CRM does not issue invoices. |
| CRM ↔ Cobranzas | Events such as `PaymentOverdue`; follow-up scheduling. | CRM does not own collection ledger. |
| CRM ↔ Marketing | Campaign and segment handoff. | Ownership may move to future Marketing module. |
| CRM ↔ Notificaciones | Portal Notification adapter only. | No CRM SMTP/Teams/WhatsApp engine in P1. |
| CRM ↔ Auditoría | Portal Audit adapter only. | No custom audit store. |
| CRM ↔ Reporting/BI | Reporting events/read models later. | No production BI in P1. |

## Conceptual events

- `LeadCreated`
- `OpportunityWon`
- `CustomerConverted`
- `InvoiceCreated`
- `PaymentOverdue`
- `FollowUpScheduled`

## Event-based integration

Events should carry identifiers and sanitized metadata, not raw sensitive documents, certificates, tokens, XML reales or full personal data.

## API-based integration

APIs should be used for explicit read models or commands after ownership is approved. API contracts must reuse Portal Gateway/Auth and avoid direct domain-to-domain database coupling.

## Out of P1

- Real event bus wiring.
- CRM API endpoints.
- CRM workers.
- CRM database.
- CRM frontend.
- Real Portal menu/permissions.
- Financiero mutation commands from CRM.
- Production notification or SRI actions.
