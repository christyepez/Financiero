# Sprint 8 P2 E2E Execution Evidence

## Result

Partial validation with infrastructure blockers. Evidence was captured without inventing Portal or SQL results.

## Environment used

- Repository: Financiero.
- Base commit: `43d850b3f9867982b988560c293d1a658fee9b20`.
- Branch: `financiero-sprint-8-p2-real-portal-e2e-execution`.
- Data: synthetic/local only.
- Secrets/certificates/XML: not used.

## Services expected

- Shared SQL Server on `host.docker.internal:21433`.
- PortalCorporativo Gateway/Shell on local Portal URL.
- Financiero API on `http://localhost:8083`.
- Financiero Angular local build.
- Optional Seq for correlation/log inspection.

## Commands executed

- `git remote -v`.
- `git checkout main`.
- `git fetch origin`.
- `git pull origin main`.
- `git rev-parse HEAD`.
- `git status`.
- `git checkout -b financiero-sprint-8-p2-real-portal-e2e-execution`.
- `tools/validate-portal-financiero-e2e.ps1`.

Additional build/test commands are tracked in the PR validation notes.

## PortalCorporativo result

Blocked. The local Portal Gateway/Shell endpoint was not reachable during the scripted validation.

## Financiero API result

Blocked at runtime because the shared SQL Server endpoint was not reachable. This is the same expected infrastructure blocker observed in Sprint 7 P5 and Sprint 8 P1.

## Financiero Angular result

Static/frontend validation is expected through `pnpm run build`, `pnpm test`, `node tools/verify-frontend.mjs` and `node tools/verify-portal-e2e-contract.mjs`.

## SQL common result

Blocked. `Test-NetConnection host.docker.internal -Port 21433` returned unavailable in the validation script.

## Gateway/Shell result

Blocked. Portal Gateway/Shell local health endpoint was not reachable.

## PortalShellContext result

Static validation available. Real PortalShellContext runtime validation remains blocked until PortalCorporativo Gateway/Shell is running.

## Menu and permissions result

Static validation available through allow-list and permission verifiers. Runtime validation remains blocked by Portal availability.

## Feature flags result

Static validation confirms dangerous flags remain false:

- `allowProductiveActivation`.
- `allowOfficialTaxFlows`.
- `allowSriSubmission`.
- `allowAtsOfficialActions`.
- `allowEvidenceUpload`.
- `allowNotificationSend`.

## Correlation id result

Script sends synthetic correlation id to Financiero readiness, but endpoint runtime was unreachable because the API container exited after failing SQL startup.

## Readiness endpoint result

`/api/financial/portal-integration/readiness` is implemented and statically validated. Runtime call is blocked by SQL/common API startup dependency in the local environment.

## External approvals readiness

Not executed at runtime due to SQL/API blocker.

## Purchases/voided readiness

Not executed at runtime due to SQL/API blocker.

## Actual blockers

- Shared SQL Server unavailable at `host.docker.internal:21433`.
- Portal Gateway/Shell unavailable at expected local Portal URL.
- Financiero API health unavailable because SQL startup dependency fails.

## Workarounds

- Start the shared SQL Server from the common environment before starting Financiero.
- Confirm `FinancieroDb` exists as a logical database in the shared SQL Server.
- Start PortalCorporativo Gateway/Shell before running the E2E script.
- Rerun `tools/validate-portal-financiero-e2e.ps1`.

## Evidence of no production

- No SRI Production.
- No SRI Test real send.
- No official ATS.
- No legal-final RIDE.
- No productive XAdES.
- No certificates.
- No real XML.
- No real evidence upload.
- No real notification send.
- No SQL Server propio in Financiero.

## Final result

Partial validation with blockers.
