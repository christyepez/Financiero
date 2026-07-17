# Financial Sprint 9 Final Infrastructure Evidence

## Purpose

Consolidate P1-P4 infrastructure evidence and prevent false PASS claims.

## Final result

`BLOCKED_DEPENDENCY`.

## Evidence by capability

| Capability | Status | Evidence |
|---|---|---|
| Shared SQL | `BLOCKED_DEPENDENCY` | `host.docker.internal` resolves; TCP `21433` remains closed. |
| Portal Gateway | `BLOCKED_DEPENDENCY` | Gateway health route `/health` returned HTTP 404 or remains unconfirmed. |
| Portal Shell | `BLOCKED_DEPENDENCY` | No live Shell URL and PortalShellContext evidence. |
| Financiero API | PARTIAL | Compose validates; runtime not forced to PASS while SQL is blocked. |
| Financiero Angular/static verification | PASS | Angular build/verifiers pass using bundled Node. |
| Preflight script | PASS/PARTIAL | Script executes and correctly exits `2` for external `BLOCKED_DEPENDENCY`. |

## Health routes configured

- Financial API: `/health`.
- Portal Gateway: `/health`.
- Portal Shell: `/health` when Shell URL is provided.

## Sanitized preflight result

- SQL: `HOST_RESOLVES_BUT_PORT_CLOSED`.
- Portal Gateway: `HTTP_STATUS_UNEXPECTED` for health route until owner confirms route.
- Financial API: `HTTP_ENDPOINT_UNREACHABLE` when not started because shared SQL is blocked.
- Exit code: `2`.

## No false PASS rule

PASS requires:

- SQL TCP evidence for `host.docker.internal:21433`.
- Portal Gateway health HTTP 2xx at owner-confirmed route.
- Portal Shell/PortalShellContext live evidence.
- Financiero health/readiness HTTP 2xx after SQL is available.
- Sanitized evidence only.

## Sanitization

No secrets, certificates, XML reales, taxpayer data, SRI responses, tokens, passwords, connection strings, `.env`, `node_modules`, `dist`, `.angular` or generated logs are included.

Control tokens: BLOCKED_DEPENDENCY; Portal Gateway; shared SQL; not production-ready; No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.
