# Financiero

Dominio de contabilidad y cumplimiento tributario/SRI, consumidor de PortalCorporativo.

Estado: Sprint 1 Accounting Core cerrado en P6. La solución .NET 8, Clean Architecture, base lógica `FinancieroDb`, health/readiness, JWT, logging/correlationId, autorización runtime por permisos, migraciones versionadas, adaptadores Portal dev, Plan de Cuentas, Años/Periodos fiscales, Asientos contables, QA integrada y documentación de release están preparados.

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

No duplicar capacidades Portal ni acceder a sus bases. En local se reutiliza el único SQL Server de PortalCorporativo y Financiero mantiene su propia base lógica `FinancieroDb`. El código actual contiene Plan de Cuentas, Fiscal Periods y Journal Entries; no contiene SRI, facturación o frontend.

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
```

APIs principales:

- `/api/financial/accounts`
- `/api/financial/fiscal-years`
- `/api/financial/fiscal-periods`
- `/api/financial/journal-entries`
- `/health`, `/health/live`, `/health/ready`

Próximo Sprint recomendado: Sprint 2 — SRI & Electronic Invoicing Foundation, sujeto a definición de firma electrónica, ambientes SRI, catálogos, secuencias tributarias, XML, autorización, contingencia y almacenamiento documental.
