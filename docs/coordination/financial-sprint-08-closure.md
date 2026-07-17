# Sprint 8 Closure - Portal E2E and QA Stabilization

## Executive summary

Sprint 8 moved Financiero from Portal integration readiness documentation into executable E2E preflight, QA infrastructure stabilization and External Approval UX hardening. The final integration status is `BLOCKED_DEPENDENCY`, not application failure: the repository validates, builds and tests, but complete runtime PASS requires shared SQL and PortalCorporativo Gateway/Shell to be available.

## Scope P1-P4

| Package | Result |
|---|---|
| P1 Portal E2E readiness | Checklist, runbook, evidence template, read-only readiness endpoint and dashboard panel. |
| P2 Real E2E execution | Non-invasive execution captured partial validation and dependency blockers. |
| P3 QA infrastructure | PASS/BLOCKED_DEPENDENCY/FAIL preflight, exit codes, env template and health troubleshooting. |
| P4 External Approval UX | Foundation-only approval UX, status mapping, Portal-owned evidence metadata-only and notification no-send copy. |

## Final state

Status: `BLOCKED_DEPENDENCY`.

Ready:

- Source is synchronized with GitHub main.
- Backend restore/build/tests pass.
- Frontend build/verifiers pass.
- Docker compose config is valid.
- No Financiero SQL Server container exists.
- Productive tax flows remain blocked.

Partial:

- PortalShellContext contract is prepared and verified statically.
- External Approval UX is functionally ready for QA with synthetic data.
- Portal Content/File and Notification are represented as Portal-owned boundaries.

Blocked:

- Shared SQL not reachable locally at `host.docker.internal:21433`.
- Portal Gateway not reachable locally at `localhost:8082`.
- Portal Shell runtime PASS evidence not captured.

## External dependencies

- PortalCorporativo shared SQL container and logical `FinancieroDb`.
- Portal Gateway/Shell runtime.
- Portal Security/Menu/Configuration/Audit/Outbox/Content/File/Notification capabilities.
- Legal/tax/security approval for any future production step.

## Risks

- Treating `BLOCKED_DEPENDENCY` as PASS.
- Misinterpreting `ApprovedFoundation` as productive approval.
- Duplicating Portal capabilities inside Financiero.
- Accidentally enabling SRI, ATS, RIDE or XAdES productive flows.
- Missing E2E evidence with real Portal runtime.

## Sprint 9 recommended decision

Choose Option A: stabilize real E2E infrastructure first. The next sprint should bring up shared SQL and Portal Gateway/Shell, then rerun the preflight until PASS before adding new functional scope.

## No-production checklist

- No SRI Production.
- No SRI Test real send.
- No official ATS.
- No legal-final RIDE.
- No productive XAdES.
- No real certificates.
- No taxpayer XML or SRI real responses.
- No upload/download evidence.
- No notification send.
- No SQL Server owned by Financiero.

## Conclusion

Sprint 8 is closed as a successful readiness and governance sprint with honest dependency blockers. Financiero is not production-ready; it is ready for Sprint 9 E2E infrastructure stabilization.
