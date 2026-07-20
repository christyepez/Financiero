# Next Cycle P4 - Productization Pause Decision

Date: 2026-07-20
Decision: pause Financiero productization
Runtime result: `BLOCKED_DEPENDENCY`
PASS E2E real: NOT_READY

## Executive summary

P4 reviewed the external owner evidence intake and reran the real preflight. Financiero remains healthy locally, but the cross-system E2E gate is still blocked by external SQL/Portal dependencies. Because no accepted owner evidence exists and the preflight returned `SCRIPT_EXIT=2`, PASS E2E real cannot be claimed.

## Reason for pause

Productization must pause because continuing without accepted SQL/Portal evidence would create false readiness and pressure to duplicate platform infrastructure inside Financiero.

## Blocked dependencies

- SQL Common TCP `host.docker.internal:21433` is closed.
- `FinancieroDb` cannot be validated until shared SQL is reachable.
- Portal Gateway `/health` returns HTTP 404.
- Portal Shell live health evidence is missing.
- PortalShellContext live evidence is missing.
- Menu/permissions live evidence is missing.
- CorrelationId live evidence is missing.
- Preflight does not return `SCRIPT_EXIT=0`.

## Technical state

- Financiero API is healthy locally.
- Docker `financial-api` is healthy on `8083`.
- Backend restore/build/test pass.
- Frontend install/build/test and static Portal contract verification pass.
- Real E2E remains blocked by SQL/Portal external gates.

## Decision

- Pause Financiero productization.
- Keep the external dependency backlog open.
- Do not initiate CRM implementation that depends on real Portal/SQL PASS.
- Allow CRM discovery only if it is isolated, non-mutating, and does not depend on Financiero PASS E2E.
- Do not create SQL Server propio, Gateway propio, Shell propio, Auth/Login propio, Menu/permissions propio or token storage in Financiero.

## Condition to reactivate Financiero productization

All of the following must be true:

1. Accepted SQL owner evidence exists for shared SQL TCP and `FinancieroDb`.
2. Accepted Portal owner evidence exists for Gateway, Shell, PortalShellContext, Menu/permissions and correlationId.
3. Evidence is sanitized and traceable to date, owner, source and commands.
4. The preflight returns `SCRIPT_EXIT=0`.
5. QA records PASS E2E real as non-production only.

## Condition to initiate CRM work

CRM implementation that depends on the shared platform should wait until Portal/SQL base evidence is accepted. CRM discovery may continue only as isolated analysis and must not assume Financiero PASS or duplicate Portal capabilities.

## Risks of continuing without PASS

- False readiness.
- Duplicated platform infrastructure.
- Security/auth drift.
- Audit and observability gaps.
- CRM built on unresolved Portal/SQL base.
- Production activation without cross-system proof.

## Approval required

Product Owner and Architecture Governance must approve any change from this pause decision. Reopening PASS capture requires accepted owner evidence and `SCRIPT_EXIT=0`; no exception may convert `BLOCKED_DEPENDENCY` into PASS.
