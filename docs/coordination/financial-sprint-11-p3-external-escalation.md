## Sprint 11 P4 follow-up update

P4 confirms the P3 escalation remains unresolved: `NoResponse` / `EvidencePending`, preflight `SCRIPT_EXIT=2`, gates 1-8 `BLOCKED_DEPENDENCY`, Gate 9 PASS. PASS E2E real is NOT_READY until accepted SQL/Portal owner evidence plus `SCRIPT_EXIT=0`.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

# Financial Sprint 11 P3 External Escalation

Date: 2026-07-20  
Phase: Sprint 11 P3  
Result: `BLOCKED_DEPENDENCY`  
Owner response: `NoResponse`  
Evidence state: `EvidencePending`  
Preflight: `SCRIPT_EXIT=2`  
Production state: not production-ready.

## Executive summary

Sprint 11 P3 could not proceed to PASS capture. No accepted external evidence was received for shared SQL, `FinancieroDb`, Portal Gateway, Portal Shell, PortalShellContext, Menu/permissions or correlation id. The acceptance preflight returned `SCRIPT_EXIT=2`; therefore Financiero must remain blocked and must not create duplicated Portal or infrastructure capabilities.

## Evidence faltante

| Dependency | Pending owner | Required evidence | Current state |
|---|---|---|---|
| shared SQL TCP | SQL Common / Infra Owner | Sanitized TCP PASS for the common SQL endpoint | EvidencePending |
| `FinancieroDb` | SQL Common / DBA Owner | Separate logical DB availability proof | EvidencePending |
| Portal Gateway health | Portal Gateway Owner | Accepted health/readiness route returning HTTP 2xx | EvidencePending |
| Portal Shell health | Portal Shell Owner | Live Shell URL and health evidence | EvidencePending |
| PortalShellContext live | Portal Contract Owner | Sanitized context sample | EvidencePending |
| Menu/permissions live | Portal Security/Menu Owner | Portal-owned menu/resources/permissions proof | EvidencePending |
| Correlation id live | Portal Observability Owner | Sanitized cross-service trace/header evidence | EvidencePending |

## Gates bloqueados

Gates 1-8 remain `BLOCKED_DEPENDENCY`. Gate 9 no-production guardrails remains PASS.

## Impacto

- PASS capture remains closed.
- Productization remains blocked.
- Financiero must not create SQL Server propio, Gateway propio, Portal Shell propio, login/Auth propio, token storage or duplicated Menu/Configuration/Audit/Outbox/Notification/Content/File.

## Riesgo

The main risk is a false PASS claim without accepted owner evidence or without `SCRIPT_EXIT=0`. A second risk is solving external dependency gaps inside Financiero, which would violate Portal reuse boundaries.

## Decisión requerida

Architecture Governance and Product Owner must escalate SQL Common and Portal owners. Owners must provide sanitized evidence using the existing templates before Sprint 11 P4 can be considered.

## Fecha límite

Target owner response: 2026-07-22 for SQL/Gateway critical gates and 2026-07-23 for Shell/PortalShellContext/Menu/correlation gates.

## Condición para desbloqueo

Accepted external owner evidence plus acceptance preflight returning `SCRIPT_EXIT=0`.

## Recomendación

Keep productization blocked. Continue external follow-up. Do not activate production and do not duplicate Portal capabilities.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.
