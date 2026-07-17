# Sprint 8 P1 E2E Evidence Template

## Environment

- Date:
- Operator:
- Machine:
- Branch:
- Commit:
- PortalCorporativo commit:
- Financiero commit:

## Services

| Service | URL/port | Status | Notes |
|---|---|---|---|
| Shared SQL Server | | | |
| Portal Gateway/Shell | | | |
| Financiero API | | | |
| Financiero Angular | | | |
| Seq | | | |

## SQL common validation

- Single SQL Server container:
- `PortalCorporativoDb` present:
- `FinancieroDb` present:
- No shared domain database:

## Portal gateway validation

- Gateway reachable:
- Correlation id forwarded:
- No token in querystring:

## Financiero API validation

- `/health`:
- `/health/live`:
- `/health/ready`:
- `/health/sri`:
- `/health/content-file`:
- `/api/financial/portal-integration/readiness`:

## Shell context test

- `contractVersion=1.0`:
- `source=portal`:
- required fields present:
- warnings sanitized:
- standalone production blocked:

## Menu test

- Allowed routes rendered:
- Unknown routes rejected:
- Permission filtering applied:

## Permission test

- read permission accepted:
- manage permission gated:
- no invented permission:

## Feature flag test

- productive activation false:
- official tax flows false:
- SRI submission false:
- ATS official actions false:
- upload false:
- notification send false:

## Readiness endpoint tests

- External approvals:
- Purchase readiness:
- Voided readiness:
- Portal integration readiness:

## Evidence of no production

- No SRI Production.
- No SRI Test real send.
- No official ATS.
- No legal-final RIDE.
- No productive XAdES.
- No certificates.
- No real XML.
- No real evidence upload.
- No notification send.

## Risks

- 

## Final result

- PASS / FAIL:
- Blockers:
- Next action:
