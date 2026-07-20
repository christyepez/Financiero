# Next Cycle P3 - Accepted Evidence Checklist

## P4 status

P4 evidence intake remains `EVIDENCE_PENDING`. No owner evidence was accepted or rejected. PASS capture stays closed and productization is paused until this checklist is satisfied and the preflight returns `SCRIPT_EXIT=0`.

Date: 2026-07-20
Purpose: define what QA may accept before reopening PASS capture.

## Required checklist

| Item | Required evidence | Status before owner response |
|---|---|---|
| SQL TCP open | `Test-NetConnection host.docker.internal -Port 21433` returns PASS. | Pending |
| `FinancieroDb` accessible | Sanitized DBA evidence that logical DB exists in shared SQL. | Pending |
| Portal Gateway health | HTTP 200 on `/health` or owner-confirmed official health route. | Pending |
| Portal Shell health | HTTP 200 on `/health` or owner-confirmed official health route. | Pending |
| PortalShellContext live | Sanitized live context evidence for Financiero. | Pending |
| Menu/permissions live | Sanitized Portal-owned resources/permissions evidence. | Pending |
| CorrelationId live | Sanitized evidence of propagation across Portal Gateway/Shell and Financiero API. | Pending |
| Financiero API health | HTTP 200 on `/health`, `/health/live`, `/health/ready`. | PASS in P2/P3 local runtime |
| Preflight | `SCRIPT_EXIT=0`. | Pending |
| Evidence sanitized | No secrets, tokens, passwords, certificates, real XML, personal data or private URLs. | Required |
| Traceability | Date, owner, source and commands included. | Required |

## Acceptance rule

PASS capture remains closed unless every runtime gate has accepted evidence and the preflight exits `0`.

## Rejection rule

Mark evidence as `REJECTED_EVIDENCE` if it contains secrets, certificates, personal data, real XML, raw tokens, private connection strings, production SRI data, unverifiable screenshots or partial health claims without commands/source.

## Do not accept as PASS

- Static frontend contract verification alone.
- Mock/development SRI flows.
- Financiero API health alone.
- Gateway 404 with no owner-approved route.
- SQL host resolution without TCP PASS.
- PortalShellContext claims without live evidence.
