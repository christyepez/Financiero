# CRM Roadmap Draft

Date: 2026-07-20
Status: draft; discovery only

## Proposed phases

| Phase | Suggested sprint | Scope | Dependencies | Risk |
|---|---|---|---|---|
| Discovery | P1 | Charter, bounded context, conceptual model, boundaries, readiness checklist. | None beyond documentation. | Low if kept isolated. |
| Foundation | Future P2 | Repo/module decision, architecture decision, data ownership. | Executive approval, Portal/SQL strategy. | Medium. |
| Core CRM | Future P3 | Leads, accounts, contacts conceptual-to-domain design. | Master Data decision, security model. | High if customer ownership unclear. |
| Pipeline/Oportunidades | Future P4 | Pipelines, stages, opportunities. | CRM foundation and configuration strategy. | Medium. |
| Actividades/Seguimiento | Future P5 | Activities, tasks, notes, interactions. | Portal Auth/Audit/Notification. | Medium. |
| Integración Financiero | Future P6 | Customer conversion and read-only financial signals. | Financiero PASS or formal decoupling. | High. |
| Reporting | Future P7 | CRM reporting/read models. | Reporting/BI boundary. | Medium. |
| Productización | Future P8 | Non-production readiness then controlled rollout. | PASS E2E, security, audit, operations. | High. |

## What can advance while Financiero remains paused

- Discovery documents.
- Conceptual domain model.
- Architecture decision preparation.
- Master data ownership analysis.
- Security and tenant model design.

## What cannot advance until Portal/SQL is resolved or decoupled

- CRM runtime.
- CRM database/migrations.
- CRM frontend.
- CRM endpoints.
- Real Portal menu/permissions.
- Integration with Financiero financial data.
- Productization.

## Next recommended step

Review P1 discovery with Product Owner and Architecture Governance, then decide whether CRM belongs in a separate repository, Portal module, or future platform bounded context.
