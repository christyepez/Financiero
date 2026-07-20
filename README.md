# Financiero

## Sprint 10 P4 external remediation follow-up

Sprint 10 P4 executed the formal follow-up of external owner escalation. No accepted SQL/Portal evidence was received, so the executive decision is to keep productization blocked and prepare P5 controlled closure if evidence remains unavailable.

- Follow-up evidence: `docs/qa/financial-sprint-10-p4-remediation-followup-evidence.md`.
- Executive block decision: `docs/coordination/financial-sprint-10-p4-executive-block-decision.md`.
- Current state: `NoResponse` / `EvidencePending` / `BLOCKED_DEPENDENCY`.
- Gate 9 no-production guardrails remains the only accepted PASS.
- No SQL Server propio, Gateway propio, Portal Shell propio, login/Auth propio, token storage, upload/download evidence or notification send.
- No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 10 P3 external owner escalation

Sprint 10 P3 formalizes escalation and remediation tracking for external SQL/Portal owners. No new runtime capability is implemented, no PASS is claimed and the state remains `EvidencePending` / `BLOCKED_DEPENDENCY`.

- Escalation matrix: `docs/coordination/financial-sprint-10-p3-owner-escalation-matrix.md`.
- Formal request: `docs/coordination/financial-sprint-10-p3-formal-evidence-request.md`.
- Remediation log: `docs/qa/financial-sprint-10-p3-external-remediation-log.md`.
- Next decision: request sanitized owner evidence, then run Sprint 10 P4 follow-up or resolve Portal/Infra outside Financiero.
- No SQL Server propio, Gateway propio, Portal Shell propio, login/Auth propio, token storage, upload/download evidence or notification send.
- No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 10 P2 owner evidence review

Sprint 10 P2 reviewed owner evidence intake and executed acceptance gates. No external SQL/Portal evidence was received, so the real state remains `BLOCKED_DEPENDENCY`.

- Evidence review: `docs/qa/financial-sprint-10-p2-owner-evidence-review.md`.
- Gate execution: `docs/qa/financial-sprint-10-p2-acceptance-gate-execution.md`.
- Preflight supports `-AcceptanceGateReport` and `-EvidenceOutputPath`.
- No PASS is claimed until SQL, Portal Gateway, Portal Shell and PortalShellContext evidence is accepted.

## Sprint 10 P1 external infra remediation kickoff

Sprint 10 starts as external infrastructure remediation, not production activation. Owner evidence intake and E2E gates are now the source of acceptance for the next PASS attempt.

- Intake: `docs/coordination/financial-sprint-10-p1-owner-evidence-intake.md`.
- Acceptance gate: `docs/qa/financial-sprint-10-p1-e2e-acceptance-gate.md`.
- Evidence templates: `docs/qa/templates/`.
- Current state remains `BLOCKED_DEPENDENCY` until owner evidence is accepted.
- No SQL Server propio, Gateway propio, Portal Shell propio, token storage, upload/download evidence or notification send.
- No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 9 P5 closure

Sprint 9 closes as `BLOCKED_DEPENDENCY`. The recommended Sprint 10 decision is External Infra Remediation plus Portal contract alignment before productization.

- Closure: `docs/coordination/financial-sprint-09-closure.md`.
- Final evidence: `docs/qa/financial-sprint-09-final-infra-evidence.md`.
- Sprint 10 decision: `docs/roadmap/financial-sprint-10-decision-matrix.md`.
- External backlog: `docs/roadmap/financial-external-dependency-backlog.md`.
- Release notes: `docs/releases/financial-sprint-09-release-notes.md`.

Financiero remains not production-ready: No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 9 P4 external infrastructure intervention

Current local E2E status remains `BLOCKED_DEPENDENCY`, not PASS. Sprint 9 P4 adds an external intervention package for shared SQL and Portal runtime owners, plus configurable health route validation in `tools/validate-portal-financiero-e2e.ps1`.

- SQL intervention: `docs/runbooks/infra-sql-common-intervention-package.md`.
- Portal intervention: `docs/runbooks/portal-runtime-intervention-package.md`.
- Evidence: `docs/qa/financial-sprint-09-p4-infra-intervention-evidence.md`.
- No SQL Server propio, no Gateway/Shell propio, no token storage, no upload/download evidence, no notification send.
- No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

Dominio de contabilidad y cumplimiento tributario/SRI, consumidor de PortalCorporativo.

Estado: Sprint 9 P3 intenta capturar PASS real y deja evidencia final `BLOCKED_DEPENDENCY` con handoff operativo. SQL común `host.docker.internal:21433` sigue cerrado, Portal Gateway en `localhost:8082/health` responde `HTTP 404` y Portal Shell live evidence no está disponible; no se inventa PASS. La solución .NET 8, Clean Architecture, base lógica `FinancieroDb`, health/readiness, autorización runtime, facturación electrónica foundation, validación XML endurecida, firma dev/mock controlada, contrato SRI test dry-run/manual probe, Secret Store wiring, sanitización, observabilidad segura, adapter productivo-ready hacia Portal Content/File, RIDE/PDF foundation por tipo documental, foundation de NC/ND/Retenciones, compras/anulados foundation, mapping ATS de sustentos, catálogos foundation versionados, preview ATS XML foundation gated, workflow foundation de aprobaciones externas y frontend Angular seguro quedan documentados como readiness técnico, sin producción SRI ni certificados reales.

Documentos principales:

- `codex/PROJECT_CONTEXT.md`
- `codex/PORTAL_INTEGRATION_CONTRACTS.md`
- `docs/architecture/accounting-core-architecture.md`
- `docs/architecture/decisions/adr-001-financial-portal-integration.md`
- `docs/architecture/decisions/adr-002-accounting-core.md`
- `docs/coordination/financial-sprint-01-accounting-core.md`
- `docs/coordination/financial-sprint-01-p1-chart-of-accounts.md`
- `docs/coordination/financial-sprint-01-p2-fiscal-periods.md`
- `docs/coordination/financial-sprint-01-p3-journal-entries.md`
- `docs/coordination/financial-sprint-01-p4-portal-adapters-hardening.md`
- `docs/coordination/financial-sprint-01-p5-integrated-qa-concurrency.md`
- `docs/security/runtime-permission-strategy.md`
- `docs/architecture/database-migration-strategy.md`
- `docs/architecture/sequence-concurrency-strategy.md`
- `docs/architecture/financial-sprint-01-architecture-snapshot.md`
- `docs/api/financial-api-contracts.md`
- `docs/api/financial-api-index.md`
- `docs/security/financial-permission-matrix.md`
- `docs/database/financial-database-inventory.md`
- `docs/qa/financial-sprint-01-qa-evidence.md`
- `docs/releases/financial-sprint-01-accounting-core-closure.md`
- `docs/releases/financial-sprint-01-release-notes.md`
- `docs/coordination/financial-sprint-02-backlog-readiness.md`
- `docs/coordination/financial-sprint-02-sri-electronic-invoicing-foundation.md`
- `docs/architecture/sri-electronic-invoicing-architecture.md`
- `docs/architecture/decisions/adr-003-sri-electronic-invoicing-foundation.md`
- `docs/api/financial-sri-api-contracts.md`
- `docs/security/sri-certificate-and-secrets-strategy.md`
- `docs/security/xades-signature-strategy.md`
- `docs/integration/sri-soap-contract-strategy.md`
- `docs/integration/electronic-document-storage-strategy.md`
- `docs/database/financial-sri-database-inventory.md`
- `docs/releases/financial-sprint-02-sri-readiness-closure.md`
- `docs/releases/financial-sprint-02-release-notes.md`
- `docs/architecture/financial-sprint-02-sri-architecture-snapshot.md`
- `docs/integration/sri-test-manual-validation-runbook.md`
- `docs/security/sri-secret-certificate-checklist.md`
- `docs/api/financial-sri-api-index.md`
- `docs/qa/financial-sprint-02-qa-evidence.md`
- `docs/coordination/financial-sprint-03-backlog-readiness.md`
- `docs/coordination/financial-sprint-03-p1-tax-documents-foundation.md`
- `docs/architecture/decisions/adr-006-tax-documents-foundation.md`
- `docs/coordination/financial-sprint-03-p2-sri-catalogs-tax-rules.md`
- `docs/architecture/decisions/adr-007-sri-catalogs-tax-rules.md`
- `docs/releases/financial-sprint-04-closure.md`
- `docs/releases/financial-sprint-04-release-notes.md`
- `docs/review/external-review-gate.md`
- `docs/coordination/financial-sprint-05-backlog-readiness.md`
- `docs/coordination/financial-sprint-05-p1-purchases-voided-foundation.md`
- `docs/coordination/financial-sprint-05-p2-ats-support-mapping-hardening.md`
- `docs/architecture/decisions/adr-015-ats-support-document-mapping.md`
- `docs/coordination/financial-sprint-05-p3-purchase-voided-catalogs.md`
- `docs/architecture/decisions/adr-016-tax-catalogs-foundation.md`
- `docs/coordination/financial-sprint-05-p4-ats-xml-foundation-gated.md`
- `docs/architecture/decisions/adr-017-ats-xml-foundation-gated.md`
- `docs/reporting/ats-xml-foundation-gated.md`
- `docs/coordination/financial-sprint-05-p5-closure-approval-workflow.md`
- `docs/architecture/decisions/adr-018-external-approval-workflow-foundation.md`
- `docs/coordination/financial-sprint-06-p1-angular-shell-foundation.md`
- `docs/coordination/financial-sprint-06-p2-angular-real-data-wiring.md`
- `docs/coordination/financial-sprint-06-p3-portal-shell-contract.md`
- `docs/coordination/financial-sprint-06-p4-controlled-ui-commands.md`
- `docs/coordination/financial-sprint-06-p5-closure-ux-portal-readiness.md`
- `docs/coordination/financial-sprint-07-p1-real-portal-shell-contract.md`
- `docs/coordination/financial-sprint-07-p2-external-approval-persistence.md`
- `docs/coordination/financial-sprint-07-p3-contentfile-notification-boundary.md`
- `docs/coordination/financial-sprint-07-p4-controlled-productization-readiness.md`
- `docs/coordination/financial-sprint-07-closure.md`
- `docs/qa/financial-sprint-07-qa-evidence.md`
- `docs/architecture/financial-sprint-07-capability-matrix.md`
- `docs/security/financial-sprint-07-security-checklist.md`
- `docs/roadmap/financial-sprint-08-roadmap.md`
- `docs/coordination/financial-sprint-08-closure.md`
- `docs/qa/financial-sprint-08-final-e2e-evidence.md`
- `docs/qa/financial-sprint-09-p1-real-e2e-infra-evidence.md`
- `docs/qa/financial-sprint-09-p2-dependency-diagnostic-evidence.md`
- `docs/qa/financial-sprint-09-p3-pass-or-blocked-evidence.md`
- `docs/runbooks/start-shared-sql-and-portal-runtime.md`
- `docs/runbooks/financial-e2e-pass-checklist.md`
- `docs/runbooks/financial-e2e-dependency-owner-handoff.md`
- `docs/roadmap/financial-sprint-09-decision-matrix.md`
- `docs/roadmap/financial-controlled-productization-backlog.md`
- `docs/architecture/financial-risk-register.md`
- `docs/releases/financial-sprint-08-release-notes.md`
- `docs/integration/portal-e2e-validation-checklist.md`
- `docs/runbooks/portal-financiero-local-e2e-runbook.md`
- `docs/qa/financial-sprint-08-p1-e2e-evidence-template.md`
- `docs/qa/financial-sprint-08-p2-e2e-execution-evidence.md`
- `docs/runbooks/shared-sql-runtime-validation.md`
- `docs/runbooks/financial-qa-env-template.md`
- `docs/runbooks/financial-health-troubleshooting.md`
- `docs/qa/financial-sprint-08-p3-qa-infra-stabilization-evidence.md`
- `docs/qa/financial-sprint-08-p4-external-approval-ux-evidence.md`
- `docs/frontend/financial-angular-shell.md`
- `docs/frontend/portal-shell-contract.md`
- `docs/frontend/portal-shell-readiness-matrix.md`
- `docs/releases/financial-sprint-06-closure.md`
- `docs/releases/financial-sprint-06-release-notes.md`
- `docs/qa/financial-sprint-06-qa-evidence.md`
- `docs/architecture/financial-sprint-06-architecture-snapshot.md`
- `docs/architecture/decisions/adr-019-angular-shell-portal-integration.md`
- `docs/architecture/decisions/adr-020-angular-real-data-wiring.md`
- `docs/architecture/decisions/adr-021-portal-shell-contract-integration.md`
- `docs/architecture/decisions/adr-022-controlled-ui-command-foundation.md`
- `docs/architecture/decisions/adr-023-real-portal-shell-contract-hardening.md`
- `docs/architecture/decisions/adr-024-external-approval-workflow-persistence.md`
- `docs/architecture/decisions/adr-025-portal-contentfile-notification-boundary.md`
- `docs/architecture/decisions/adr-026-controlled-productization-readiness.md`
- `docs/releases/financial-sprint-07-readiness-notes.md`
- `docs/releases/financial-sprint-07-release-notes.md`
- `docs/releases/financial-sprint-05-closure.md`
- `docs/releases/financial-sprint-05-release-notes.md`
- `docs/architecture/financial-sprint-04-architecture-snapshot.md`
- `docs/qa/financial-sprint-04-qa-evidence.md`

No duplicar capacidades Portal ni acceder a sus bases. En local se reutiliza el único SQL Server de PortalCorporativo y Financiero mantiene su propia base lógica `FinancieroDb`. El frontend Angular foundation no contiene login propio, almacenamiento local de tokens, RIDE/PDF final, firma XAdES productiva ni envío real a SRI.

Ejecución y variables: `docs/coordination/financial-local-development.md`.

Comandos principales:

```powershell
dotnet restore Financiero.sln
dotnet build Financiero.sln
dotnet test Financiero.sln
docker compose up -d --build financial-api
cd frontend/financiero-web
npm install
npm run build
npm test
```

El puerto local de la API es configurable con `FINANCIAL_API_PORT`. El ambiente validado en P5/P6 usa `http://localhost:8083`.

Smoke local:

```powershell
scripts/smoke/financial-smoke.ps1 -BaseUrl http://localhost:8083
scripts/smoke/financial-sri-smoke.ps1 -BaseUrl http://localhost:8083
```

Sprint 4 P2 agrega contrato HTTP productivo-ready para Portal Content/File. El provider local por defecto es `Development`; `PortalContentFile` requiere `SRI_STORAGE_PORTAL_BASE_URL`, usa `SRI_STORAGE_DRY_RUN=true` por defecto y no envía payloads ni tokens reales salvo configuración explícita fuera del repo.

Sprint 4 P3 agrega templates RIDE foundation por tipo documental y diseño ATS oficial foundation. Ningún output debe considerarse legal final ni ATS oficial.

Sprint 4 P4 agrega análisis read-only de gaps RIDE/ATS y checklist de aprobación. No aprueba uso productivo, no genera RIDE legal final, no genera XML ATS oficial y no persiste evidencia real.

Sprint 4 P5 cierra formalmente la etapa como readiness. La producción SRI, RIDE legal final, ATS oficial, payloads reales y XAdES productivo siguen bloqueados hasta revisión externa y gates aprobados. Docker runtime debe reintentarse cuando MCR responda.

Sprint 5 P1 agrega compras tributarias y documentos anulados foundation para reducir gaps ATS. No genera ATS oficial ni almacena XML real; usa permisos runtime existentes y Audit/Outbox del Portal.

Sprint 5 P2 agrega mapping/readiness ATS de sustentos como consultas read-only sanitizadas. No genera XML ATS oficial, no agrega migración `013` y no crea storage documental propio.

Sprint 5 P3 agrega catálogos foundation versionados para compras/anulados, expuestos como endpoints read-only. No son catálogos oficiales finales y requieren revisión tributaria.

Sprint 5 P4 agrega preview ATS XML foundation gated. Está bloqueado por defecto, no persiste XML, no envía al SRI y no afirma cumplimiento oficial.

Sprint 5 P5 cierra Sprint 5 y agrega workflow foundation de aprobaciones externas. Es read-only/advisory y no habilita producción.

APIs principales:

- `/api/financial/accounts`
- `/api/financial/fiscal-years`
- `/api/financial/fiscal-periods`
- `/api/financial/journal-entries`
- `/api/financial/electronic-documents`
- `/api/financial/electronic-documents/credit-notes`
- `/api/financial/electronic-documents/debit-notes`
- `/api/financial/electronic-documents/withholdings`
- `/api/financial/portal-integration/readiness`
- `/health`, `/health/live`, `/health/ready`, `/health/sri`

Sprint 6 P1 agrega `frontend/financiero-web` como Angular standalone read-only, con adapters reemplazables para Portal Security/Menu/Notification, clientes API sanitizados e interceptores de correlation/autorización.

Sprint 6 P2 conecta las pantallas Angular a endpoints reales existentes del backend local, desempaqueta `ApiResponse<T>`, agrega estados loading/error/empty y mantiene máscaras defensivas para identificaciones/access keys.

Sprint 6 P3 agrega contrato reemplazable de Portal Shell mediante `PortalShellContext`, providers standalone/portal-integrated, feature flags seguros e indicadores UX. No agrega login propio ni token storage.

Sprint 6 P4 agrega comandos foundation para crear/validar compras y registrar anulados, protegidos por permisos y feature flags apagados por defecto. No envía SRI, no genera ATS oficial y no muestra XML.

Sprint 6 P5 cierra el sprint con evidencia QA, release notes, architecture snapshot, matriz de readiness Portal Shell y hardening UX. Los comandos siguen apagados por defecto y producción continúa bloqueada para SRI/ATS/XML/RIDE productivo.

Sprint 7 P1 agrega `contractVersion=1.0`, `source`, capabilities, warnings, validación de contexto, menú allow-listed, delegated auth en memoria y bloqueo seguro en producción cuando falta Portal Shell real.

Sprint 7 P2 agrega requests persistidos de aprobaciones externas foundation. Guarda metadata/referencias sanitizadas y decisiones foundation; no guarda evidencia real, archivos, XML, certificados ni habilita producción.

Sprint 7 P3 agrega contratos y readiness para Portal Content/File y Notification. Financiero valida referencias seguras, prepara intents foundation sin envío real y mantiene Content/File y Notification como capacidades Portal-owned.

Sprint 7 P4 agrega readiness read-only de productización para compras y documentos anulados. Expone gates y blockers sin mutar estado ni habilitar SRI, ATS oficial, RIDE legal final, upload o notificaciones.

Sprint 7 P5 cierra Sprint 7 formalmente como readiness no productivo. Consolida QA evidence, capability matrix, checklist de seguridad, release notes y roadmap Sprint 8. La ruta recomendada es validar end-to-end con PortalCorporativo real y estabilizar SQL común/health antes de cualquier productización.

Sprint 8 P1 agrega checklist/runbook/evidence template para validación Portal E2E, endpoint read-only `/api/financial/portal-integration/readiness`, panel de dashboard y verificador estático `verify-portal-e2e-contract.mjs`. Mantiene modo standalone como development-only y producción tributaria bloqueada.

Sprint 8 P2 ejecuta el script no invasivo `tools/validate-portal-financiero-e2e.ps1` y registra partial validation: compose válido y sin SQL Server propio, pero SQL común `host.docker.internal:21433` y Portal Gateway/Shell no estaban accesibles. Se endurece el readiness endpoint/dashboard con servicios esperados, SQL común requerido y drift indicators.

Sprint 8 P3 fortalece el script con `-SkipPortalChecks`, `-SkipApiHealthChecks`, `-OutputMarkdown`, resolución de `host.docker.internal`, exit codes `0/2/1` y estados `PASS/BLOCKED_DEPENDENCY/FAIL`. También agrega plantilla QA env y troubleshooting health.

Sprint 8 P4 endurece `ExternalApprovalsComponent` con estados Draft/Submitted/InReview/ApprovedFoundation/RejectedFoundation/Blocked/Superseded/Cancelled, disclaimers explícitos, referencias de evidencia Portal-owned metadata-only, notification intents foundation/no-send, blockers visibles y próximos pasos seguros. `ApprovedFoundation` no habilita producción ni sustituye aprobación legal/tributaria.

Sprint 8 P5 cierra el sprint como `BLOCKED_DEPENDENCY`: backend/frontend/verificadores pasan, compose es válido y no existe SQL Server propio, pero falta evidencia PASS con SQL común y PortalCorporativo Gateway/Shell activos. La decisión recomendada para Sprint 9 es estabilizar infraestructura E2E real antes de ampliar alcance.

Sprint 9 P1 ejecuta nuevamente el preflight real y mantiene `BLOCKED_DEPENDENCY`: DNS de `host.docker.internal` resuelve, compose es válido y no existe SQL Server propio, pero SQL TCP `21433`, Financiero API `8083` y Portal Gateway `8082` no están disponibles. El script no requiere hardening funcional adicional; el bloqueo es de infraestructura.

Sprint 9 P2 agrega `-VerboseDiagnostics` y `-SuggestFixes` al preflight, documenta causas/acciones y crea el runbook `docs/runbooks/start-shared-sql-and-portal-runtime.md`.

Sprint 9 P3 confirma que el estado sigue `BLOCKED_DEPENDENCY`, pero con una señal más precisa: SQL host resuelve y puerto 21433 está cerrado; Portal Gateway responde en 8082 pero `/health` devuelve `HTTP 404`, por lo que se requiere confirmar health path/routing/puerto con el owner de Portal.

Próximo paso recomendado: levantar SQL común y PortalCorporativo real, ejecutar `tools/validate-portal-financiero-e2e.ps1 -OutputMarkdown -VerboseDiagnostics -SuggestFixes`, capturar evidencia PASS y mantener bloqueados SRI producción, SRI Test real, ATS oficial, RIDE legal final, XAdES productivo, upload/download de evidencia y envío real de notificaciones.
