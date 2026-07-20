# Financial Next Cycle P1 Owner/SLA Requirements

Date: 2026-07-20  
Current result: `BLOCKED_DEPENDENCY`  
External evidence: `NoResponse` / `EvidencePending`  
PASS capture: closed  
Preflight: `SCRIPT_EXIT=2`  
Production state: not production-ready.

| Dependency | Minimum owner required | Suggested SLA | Required evidence | Gate | Risk without owner | Action if SLA expires |
|---|---|---|---|---|---|---|
| SQL Common TCP | SQL Common / Infra Owner | 2 business days | Sanitized TCP PASS for common SQL endpoint | Gate 1 | No database/runtime PASS | Escalate or pause productization |
| `FinancieroDb` | SQL Common / DBA Owner | 2 business days after SQL TCP | Separate logical DB availability proof | Gate 2 | DB readiness cannot be accepted | Escalate DBA ownership |
| Portal Gateway health | Portal Gateway Owner | 2 business days | Accepted health/readiness route HTTP 2xx | Gate 3 | Gateway reuse cannot be accepted | Escalate Portal lead |
| Portal Shell health | Portal Shell Owner | 3 business days | Live Shell URL/health evidence | Gate 4 | Shell integration cannot pass | Escalate Portal Shell owner |
| PortalShellContext | Portal Contract Owner | 3 business days | Sanitized context sample | Gate 5 | Delegated auth/menu context unaccepted | Freeze PASS capture |
| Menu/permissions | Portal Security/Menu Owner | 3 business days | Portal-owned resources/permissions proof | Gate 6 | Authorization readiness unaccepted | Escalate Security/Menu owner |
| Correlation id | Portal Observability Owner | 3 business days | Sanitized cross-service trace/header evidence | Gate 7 | Traceability cannot be accepted | Escalate Observability owner |
| Preflight `SCRIPT_EXIT=0` | Joint QA / Infra / Portal Owners | After all evidence | Full sanitized preflight output | Gate 8 | PASS E2E real unavailable | Keep PASS capture closed |

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.
