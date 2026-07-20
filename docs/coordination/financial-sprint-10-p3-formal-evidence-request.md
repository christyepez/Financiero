# Sprint 10 P3 Formal Evidence Request

Date: 2026-07-20  
Audience: SQL Common, Portal Gateway, Portal Shell, Portal Contract, Security/Menu and Observability owners.  
Status requested: sanitized evidence delivery for `EvidencePending` dependencies.

## Message ready to send

Financiero Sprint 10 remains `BLOCKED_DEPENDENCY` after P2 because no external SQL/Portal owner evidence was received. We cannot claim E2E PASS, productize, enable SRI Test real, SRI Production, official ATS, legal-final RIDE or productive XAdES until the shared SQL and Portal runtime contracts are proven with sanitized evidence.

Please provide the following evidence by the target date agreed in `docs/coordination/financial-sprint-10-p3-owner-escalation-matrix.md`:

The operational SLA is tracked in the escalation matrix and must be treated as the trigger for Sprint 10 P4 follow-up if evidence remains pending.

1. SQL Common TCP evidence: shared SQL host/port reachable from the local environment.
2. SQL `FinancieroDb` evidence: sanitized proof that the logical database exists and is accessible.
3. Portal Gateway evidence: health/readiness route, port and HTTP 2xx result.
4. Portal Shell evidence: live Shell URL/health path and HTTP 2xx result.
5. PortalShellContext evidence: sanitized live contract sample with capabilities/menu/permissions context and no tokens.
6. Menu/permissions evidence: read-only proof that Financiero resources are available through Portal-owned Security/Menu.
7. Correlation id evidence: sanitized proof of correlation id propagation across Portal and Financiero.
8. Joint PASS evidence: preflight exit code 0 only after the above evidence is accepted.

## Templates available

- `docs/qa/templates/sql-common-evidence-template.md`
- `docs/qa/templates/portal-gateway-evidence-template.md`
- `docs/qa/templates/portal-shell-evidence-template.md`
- `docs/qa/templates/portal-contract-evidence-template.md`
- `docs/coordination/financial-sprint-10-p1-owner-evidence-intake.md`
- `docs/qa/financial-sprint-10-p2-acceptance-gate-execution.md`

## Suggested command

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File tools/validate-portal-financiero-e2e.ps1 -OutputMarkdown -VerboseDiagnostics -SuggestFixes -PortalGatewayHealthPath /health -PortalShellHealthPath /health -FinancialApiHealthPath /health -EvidenceOutputPath <TEMP_PATH_OUTSIDE_REPO> -AcceptanceGateReport
```

## Do not send

- Passwords, tokens, secrets, full connection strings or private URLs.
- Real certificates, `.p12`, `.pfx`, `.pem`, `.key`, `.cer`, `.crt`.
- XML reales, datos personales, real SRI responses or production payloads.
- Screenshots with active sessions or bearer tokens.
- Requests to create SQL Server propio, Gateway propio, Shell propio, login/Auth propio or duplicate Portal capabilities in Financiero.

## Acceptance criteria

- Evidence is sanitized and references only the relevant dependency.
- Evidence confirms the owner-controlled service, route, database or contract.
- Evidence can be mapped to a specific gate in the P2 acceptance report.
- Preflight can be re-run without secrets and without committing generated evidence.

## If evidence is not received

Financiero will remain `BLOCKED_DEPENDENCY`. The recommended decision is to run Sprint 10 P4 as external remediation follow-up or resolve the Portal/Infra gap outside the Financiero repository before productization.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES. Financiero remains not production-ready.
