# Financial Sprint 10 P1 Owner Evidence Intake

## Purpose

Start Sprint 10 as external infrastructure remediation. This intake tracks owners, required evidence, state and acceptance criteria while keeping Financiero as `BLOCKED_DEPENDENCY` until real evidence is received.

## Allowed states

`NotStarted`, `Requested`, `EvidencePending`, `EvidenceReceived`, `Accepted`, `Rejected`, `Blocked`, `OutOfScope`.

## Owner intake matrix

| Owner | Dependency | Evidence required | Target date | State | Blocker | Next action | Acceptance criteria |
|---|---|---|---|---|---|---|---|
| SQL Common Owner | Shared SQL TCP `host.docker.internal:21433` | Sanitized TCP PASS and SQL reachability proof. | TBD | Requested | Port closed. | Return SQL template. | TCP PASS; no secrets; no Financiero SQL container. |
| SQL Common Owner | `FinancieroDb` logical database | Sanitized DB existence/init evidence. | TBD | Requested | DB not confirmed. | Confirm logical DB. | Separate logical DB available. |
| Portal Gateway Owner | Gateway health route | Base URL, confirmed health path and HTTP 2xx. | TBD | Requested | `/health` HTTP 404/unconfirmed. | Return Gateway template. | Owner-confirmed route PASS. |
| Portal Shell Owner | Portal Shell runtime | Shell URL and HTTP 2xx health/base evidence. | TBD | EvidencePending | No live Shell evidence. | Return Shell template. | Shell reachable without secrets. |
| Portal Contract Owner | PortalShellContext | Sanitized context: version, source, menu, permissions, feature flags, correlation id. | TBD | EvidencePending | Contract sample missing. | Return Contract template. | Contract accepted by Financiero allow-list. |
| Financiero Owner | Preflight and static verification | Preflight output, compose, build/tests, verifiers and no-production scans. | TBD | Requested | External dependencies blocked. | Rerun after owner evidence. | Preflight exit `0`; all gates PASS. |

## Current P1 state

`BLOCKED_DEPENDENCY`.

No production activation is allowed. No SQL Server propio, Gateway propio, Portal Shell propio, token storage, upload/download evidence or notification send.

Control tokens: BLOCKED_DEPENDENCY; Portal Gateway; shared SQL; not production-ready; No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.
