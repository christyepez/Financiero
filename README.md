# Financiero

Dominio de contabilidad y cumplimiento tributario/SRI, consumidor de PortalCorporativo.

Estado: Sprint 6 P4 agrega comandos UI foundation controlados para compras y anulados, deshabilitados por defecto. La solución .NET 8, Clean Architecture, base lógica `FinancieroDb`, health/readiness, autorización runtime, facturación electrónica foundation, validación XML endurecida, firma dev/mock controlada, contrato SRI test dry-run/manual probe, Secret Store wiring, sanitización, observabilidad segura, adapter productivo-ready hacia Portal Content/File, RIDE/PDF foundation por tipo documental, foundation de NC/ND/Retenciones, compras/anulados foundation, mapping ATS de sustentos, catálogos foundation versionados, preview ATS XML foundation gated, workflow foundation de aprobaciones externas y frontend Angular seguro quedan documentados como readiness técnico, sin producción SRI ni certificados reales.

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
- `docs/frontend/financial-angular-shell.md`
- `docs/frontend/portal-shell-contract.md`
- `docs/architecture/decisions/adr-019-angular-shell-portal-integration.md`
- `docs/architecture/decisions/adr-020-angular-real-data-wiring.md`
- `docs/architecture/decisions/adr-021-portal-shell-contract-integration.md`
- `docs/architecture/decisions/adr-022-controlled-ui-command-foundation.md`
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
- `/health`, `/health/live`, `/health/ready`, `/health/sri`

Sprint 6 P1 agrega `frontend/financiero-web` como Angular standalone read-only, con adapters reemplazables para Portal Security/Menu/Notification, clientes API sanitizados e interceptores de correlation/autorización.

Sprint 6 P2 conecta las pantallas Angular a endpoints reales existentes del backend local, desempaqueta `ApiResponse<T>`, agrega estados loading/error/empty y mantiene máscaras defensivas para identificaciones/access keys.

Sprint 6 P3 agrega contrato reemplazable de Portal Shell mediante `PortalShellContext`, providers standalone/portal-integrated, feature flags seguros e indicadores UX. No agrega login propio ni token storage.

Sprint 6 P4 agrega comandos foundation para crear/validar compras y registrar anulados, protegidos por permisos y feature flags apagados por defecto. No envía SRI, no genera ATS oficial y no muestra XML.

Próximo paso recomendado: integrar el Angular Shell real del Portal cuando sus contratos de Security/Menu/Configuration estén congelados y avanzar hacia UX operativa gated sin habilitar producción SRI.
