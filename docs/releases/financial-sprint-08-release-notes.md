# Sprint 8 Release Notes

## Summary

Sprint 8 delivers Portal E2E readiness, executable preflight, QA infrastructure stabilization, External Approval UX hardening and final governance closure. The release is not production-ready.

## PRs P1-P4

- P1: Portal E2E readiness foundation.
- P2: real Portal E2E execution and partial evidence.
- P3: QA preflight stabilization for shared SQL and Portal runtime.
- P4: External Approval UX hardening.

## Delivered capabilities

- `/api/financial/portal-integration/readiness` read-only posture.
- Dashboard Portal E2E readiness.
- Non-invasive preflight script with PASS/BLOCKED_DEPENDENCY/FAIL.
- Shared SQL runbooks and health troubleshooting.
- External Approval UX with explicit foundation-only warnings.
- Sprint 9 decision matrix and controlled productization backlog.

## Validation

- Backend restore/build/tests pass.
- Frontend build/verifiers pass.
- Docker compose config passes.
- Static security checks preserve no token storage, no upload/download and no notification send.
- Final runtime status remains `BLOCKED_DEPENDENCY` until shared SQL and Portal Gateway/Shell are available.

## Risks and blockers

- Shared SQL is unavailable in local final evidence.
- Portal Gateway/Shell are unavailable in local final evidence.
- No live PortalShellContext PASS evidence has been captured.

## Sprint 9 recommendation

Select infrastructure E2E stabilization first. Capture PASS with shared SQL, Portal Gateway/Shell, Financiero API health/readiness and Angular Portal context before expanding product scope.

## Not production-ready

No SRI Production, SRI Test real send, official ATS, legal-final RIDE, productive XAdES, real certificates, taxpayer XML, upload/download evidence, notification send or production activation is enabled.

Control tokens: No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

Post-closure Sprint 9 P1 note: the recommended Option A was started and still reports `BLOCKED_DEPENDENCY` because shared SQL and Portal Gateway/Shell are not available locally.
