# Financiero

Dominio de contabilidad y cumplimiento tributario/SRI, consumidor de PortalCorporativo.

Estado: Sprint 2 P5 Real Secret Store Wiring / SRI Test Manual Connectivity / Observability Hardening implementado sobre Sprint 1 Accounting Core. La solución .NET 8, Clean Architecture, base lógica `FinancieroDb`, health/readiness, autorización runtime, documentos electrónicos SRI foundation, validación XML endurecida, firma dev/mock controlada, contrato SRI test dry-run/manual probe, Secret Store wiring, sanitización, observabilidad segura, storage placeholder delegado a Portal Content/File y RIDE/PDF Development están preparados.

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

No duplicar capacidades Portal ni acceder a sus bases. En local se reutiliza el único SQL Server de PortalCorporativo y Financiero mantiene su propia base lógica `FinancieroDb`. No contiene frontend, RIDE/PDF, firma XAdES productiva ni envío real a SRI.

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

APIs principales:

- `/api/financial/accounts`
- `/api/financial/fiscal-years`
- `/api/financial/fiscal-periods`
- `/api/financial/journal-entries`
- `/api/financial/electronic-documents`
- `/health`, `/health/live`, `/health/ready`, `/health/sri`

Próximo paso recomendado: Sprint 2 P6 — ejecutar validación SRI Test controlada con credenciales no productivas fuera del repositorio.
