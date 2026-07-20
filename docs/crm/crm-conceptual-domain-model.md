# CRM Conceptual Domain Model

Date: 2026-07-20
Status: conceptual only; no code

| Entity | Description | Conceptual attributes | Relationships | Preliminary rules | Open questions |
|---|---|---|---|---|---|
| Lead / Prospecto | Potential customer not yet qualified. | source, status, interest, owner, created date. | May convert to Account/Contact/Opportunity. | Must not become financial customer automatically. | What lead sources are required first? |
| Cuenta / Empresa | Organization or commercial account. | name, identification reference, industry, size, owner. | Has contacts, opportunities, activities. | Should not duplicate Master Data if one exists. | Is account identity owned by CRM or Master Data? |
| Contacto | Person linked to account/prospect. | name, role, channels, preferences. | Belongs to account; participates in interactions. | Personal data requires security/privacy rules. | Which fields are allowed without PII risk? |
| Oportunidad | Potential sale or commercial deal. | amount estimate, probability, stage, close date. | Belongs to account; moves through pipeline. | Winning may trigger customer conversion event. | Which opportunity types exist? |
| Pipeline | Ordered commercial process. | name, type, active flag. | Contains stages. | Must be configurable later, not hardcoded in UI. | Portal Configuration or CRM config? |
| Etapa comercial | Step in a pipeline. | name, order, probability, terminal flag. | Part of pipeline; used by opportunities. | Terminal stages include won/lost. | Need multiple pipelines by tenant? |
| Actividad | Commercial action performed or planned. | type, due date, owner, status. | Linked to lead, account, contact or opportunity. | Should support audit via Portal later. | Which activity types matter first? |
| Tarea | Action item assigned to a user. | title, assignee, due date, status. | May be derived from activity/follow-up. | Must reuse Portal identity/permissions. | Is tasking owned by CRM or Portal workflow? |
| Nota | Internal relationship note. | text, author, timestamp, visibility. | Linked to CRM records. | Sensitive notes require redaction/audit rules. | Need private/team notes? |
| Interacción | Communication touchpoint. | channel, direction, summary, timestamp. | Linked to contacts/accounts/opportunities. | No real message sending in P1. | Which channels are allowed? |
| Campaña | Marketing/commercial initiative. | name, objective, segment, period. | Creates leads or targets segments. | Marketing may own campaign execution later. | Is campaign execution in CRM or Marketing module? |
| Segmento | Grouping criteria for targeting. | criteria, lifecycle, owner. | Used by campaigns and reporting. | Must avoid storing raw sensitive criteria unnecessarily. | Where are segmentation rules stored? |
| Cliente convertido | Prospect/account accepted as customer. | conversion date, source opportunity, external customer ref. | Links CRM to future Master Data/Financiero customer. | Conversion must not create accounting records directly. | Who owns customer canonical ID? |

## Future relationship with Cliente Financiero

CRM should reference a financial customer by stable external identifier after conversion. Financiero remains owner of invoices, collections, tax and accounting state.

## Future relationship with Factura / Cobranza / Estado de cuenta

CRM may consume summarized events or read-only views: invoice created, payment overdue, account statement available, payment received. CRM should not create tax documents or write collections balances directly.

## Open model questions

- Should customer identity be owned by CRM, Financiero, Portal Catalog, or a future Master Data service?
- Are tenants required from day one?
- Which CRM fields are sensitive and require masking/audit?
- Which events are required before any API contract is designed?
