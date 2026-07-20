# CRM Discovery P1 - Charter

Date: 2026-07-20
Phase: CRM Discovery P1
Status: discovery only

## Executive summary

CRM starts as isolated discovery only. Financiero productization remains paused because real Portal/SQL E2E PASS is not available. This discovery defines scope and architecture boundaries without creating CRM runtime, backend, frontend, database, migrations, endpoints, menu entries or permissions.

## Discovery objective

Define what CRM should own, how it may relate to Financiero and Portal later, and what evidence is required before implementation can start safely.

## Why CRM is not implemented yet

- Financiero PASS E2E real is `NOT_READY`.
- Shared SQL and Portal runtime evidence remains blocked.
- CRM implementation that depends on real Portal/SQL PASS is blocked.
- Creating CRM runtime now would risk duplicating Portal capabilities or building on an unresolved platform base.

## Dependency state

| Area | Current state |
|---|---|
| Financiero | Healthy locally but productization paused. |
| Portal/SQL | External evidence pending; preflight remains `SCRIPT_EXIT=2`. |
| CRM | Discovery only; no runtime. |

## Isolation decision

CRM is treated as a separate bounded context candidate, not as a feature embedded inside Financiero billing/tax flows. The current repository may host discovery documentation only until the target repo/module and platform readiness are approved.

## Allowed in P1

- Conceptual domain modeling.
- Bounded-context boundaries.
- Future integration mapping.
- Readiness checklist.
- Roadmap draft.
- Risk and governance updates.

## Not allowed in P1

- CRM code, endpoints, routes, migrations, tables, frontend screens, real menu entries, real permissions or runtime containers.
- SQL Server propio, Gateway propio, Portal Shell propio, login/Auth propio or token storage.
- Production SRI, official ATS, legal-final RIDE, productive XAdES, secrets, certificates, XML reales or personal data.

## Expected discovery result

A clear, reviewable CRM discovery baseline that can be used to decide whether CRM becomes a separate repository, a Portal module, or a bounded context in a broader platform architecture.

## Criteria to move to implementation

- Executive decision approves CRM implementation path.
- Target repo/module is defined.
- Portal Auth/Menu/permissions and shared SQL/DB strategy are ready or explicitly decoupled.
- Master data ownership is decided.
- Security, tenancy, audit, notification and reporting boundaries are approved.
- Financiero PASS dependency is either resolved or formally decoupled.
