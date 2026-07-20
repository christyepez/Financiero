# Next Cycle P2 - Dependency Root Cause Matrix

Date: 2026-07-20  
Final state: `BLOCKED_DEPENDENCY`

| Dependency | Status | Command used | Observed result | Probable cause | Suggested owner | Corrective action | Can be solved in Financiero | Risk | Priority |
|---|---|---|---|---|---|---|---|---|---|
| SQL Common TCP | `BLOCKED_DEPENDENCY` | `Test-NetConnection host.docker.internal -Port 21433` | Host resolves, TCP connect fails. | Shared SQL is stopped, not mapped to 21433, or blocked by firewall. | SQL Common / Infra | Start shared SQL, expose 21433, validate firewall and Docker Desktop networking. | No | Blocks real DB/E2E. | High |
| `FinancieroDb` | `BLOCKED_DEPENDENCY` | Preflight gate | Requires SQL TCP PASS and owner DB evidence. | Logical DB cannot be proven while TCP is closed. | DBA / SQL Common | Confirm database exists in shared SQL, not in a domain-specific SQL container. | No | Blocks persistence proof. | High |
| Portal Gateway `/health` | `BLOCKED_DEPENDENCY` | `Invoke-WebRequest http://localhost:8082/health` | HTTP 404. | Gateway is running without `/health`, wrong service/port, or route not exposed. | Portal Gateway Owner | Confirm health route and port or fix Portal Gateway health endpoint. | No | Blocks Portal-integrated PASS. | High |
| Portal Shell `/health` | `BLOCKED_DEPENDENCY` | Preflight | `PortalShellBaseUrl` not provided. | Shell runtime/evidence absent. | Portal Shell Owner | Start Shell, provide health URL and sanitized evidence. | No | Blocks live shell integration. | High |
| PortalShellContext | `BLOCKED_DEPENDENCY` | Preflight gate | Owner evidence required. | No live context evidence provided. | Portal Contract Owner | Provide live PortalShellContext with menu, permissions and correlation evidence. | No | Blocks runtime contract acceptance. | High |
| Menu/permissions | `BLOCKED_DEPENDENCY` | Preflight gate | Owner evidence required. | Portal-owned security/menu runtime not evidenced. | Portal Security/Menu Owner | Provide sanitized live evidence for expected resources/permissions. | No | Risk of duplicated auth/menu if bypassed. | High |
| CorrelationId | `BLOCKED_DEPENDENCY` | Preflight gate | Owner evidence required. | Portal-to-Financiero correlation path not evidenced. | Portal Observability Owner | Provide live correlation evidence across Gateway/Shell/API. | No | Weak traceability. | Medium |
| Financiero API `/health` | `PASS` | `Invoke-WebRequest http://localhost:8083/health` | HTTP 200 after Docker start. | N/A | Financiero DevOps | Keep current Docker service; do not add SQL propio. | Yes | Low. | Medium |
| Financiero API `/api/financial/portal-integration/readiness` | `PASS` | Preflight | HTTP 200 after Docker start. | N/A | Financiero Backend | Keep readiness endpoint as consumer-side status only. | Yes | Low; do not treat as Portal PASS. | Medium |
| Frontend Angular | `PASS` | `pnpm run build`; `pnpm test`; `node tools/verify-portal-e2e-contract.mjs` | Passed with Node runtime on PATH. | N/A | Frontend Agent | Keep static verifier; live Shell still needs Portal evidence. | Yes | Medium if confused with live Shell PASS. | Medium |
| Preflight `SCRIPT_EXIT=0` | `BLOCKED_DEPENDENCY` | `tools/validate-portal-financiero-e2e.ps1` | `SCRIPT_EXIT=2`; PASS=12, BLOCKED_DEPENDENCY=2, FAIL=0. | SQL and Portal external gates remain blocked. | QA Lead + External Owners | Re-run only after SQL and Portal evidence are available. | No | Blocks PASS E2E real. | High |

## Conclusion

Financiero runtime is locally activatable in Docker, but real Portal/SQL E2E remains blocked by external dependencies. The correct path is external remediation, not duplicating SQL/Gateway/Shell/Auth/Menu capabilities inside Financiero.

## P3 owner remediation mapping

| External owner | Package | Evidence expected | Next validation |
|---|---|---|---|
| SQL Common / Infra + DBA | `docs/coordination/financial-next-cycle-p3-sql-owner-remediation-package.md` | SQL TCP PASS and `FinancieroDb` accessible in shared SQL. | Preflight Gate 1 and Gate 2. |
| Portal Gateway / Shell / Contract / Security-Menu / Observability | `docs/coordination/financial-next-cycle-p3-portal-owner-remediation-package.md` | Gateway/Shell health, PortalShellContext, Menu/permissions and correlationId live evidence. | Preflight Gate 3, Gate 4, Gate 5 and Gate 7. |
| QA Lead | `docs/qa/financial-next-cycle-p3-accepted-evidence-checklist.md` | Sanitized owner evidence with date, owner, source and commands. | Reopen PASS capture only with `SCRIPT_EXIT=0`. |

## P4 decision mapping

No accepted owner evidence was received for the SQL or Portal rows above. The P4 decision is to pause productization, keep the external backlog open, and rerun PASS capture only after evidence is accepted and preflight exits `0`.
