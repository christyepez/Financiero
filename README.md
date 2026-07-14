# Financiero

Dominio de contabilidad y cumplimiento tributario/SRI, consumidor de PortalCorporativo.

Estado: Sprint 4 P4 Tax/Legal Review Gaps implementado sobre Sprint 4 P3. La solución .NET 8, Clean Architecture, base lógica `FinancieroDb`, health/readiness, autorización runtime, facturación electrónica foundation, validación XML endurecida, firma dev/mock controlada, contrato SRI test dry-run/manual probe, Secret Store wiring, sanitización, observabilidad segura, adapter productivo-ready hacia Portal Content/File, RIDE/PDF foundation por tipo documental, foundation de NC/ND/Retenciones, catálogos internos, reglas tributarias, reporting avanzado, exports JSON/CSV foundation, ATS readiness interno, ATS official design foundation y gestión de gaps tributarios/legales quedan documentados como readiness técnico, sin producción SRI ni certificados reales.

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

No duplicar capacidades Portal ni acceder a sus bases. En local se reutiliza el único SQL Server de PortalCorporativo y Financiero mantiene su propia base lógica `FinancieroDb`. No contiene frontend, RIDE/PDF final, firma XAdES productiva ni envío real a SRI.

Ejecución y variables: `docs/coordination/financial-local-development.md`.

Comandos principales:

```powershell
dotnet restore Financiero.sln
dotnet build Financiero.sln
dotnet test Financiero.sln
docker compose up -d --build financial-api
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

Próximo paso recomendado: revisión externa tributaria/legal Ecuador y planificación de Sprint 4 P5 para cerrar gaps aprobados sin activar producción.
