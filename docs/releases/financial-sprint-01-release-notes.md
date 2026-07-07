# Financiero Sprint 1 — Release Notes

## Resumen ejecutivo

Sprint 1 entrega el núcleo contable inicial de Financiero sobre la plataforma transversal PortalCorporativo. La solución permite administrar cuentas, períodos fiscales y asientos contables, con seguridad runtime, auditoría/outbox por puertos, health/readiness, migraciones versionadas y QA integrada.

## Componentes entregados

- Bootstrap Financiero: solución .NET 8, Clean Architecture, Docker Compose y configuración local.
- Chart of Accounts: cuentas contables, árbol, activación, desactivación y archivo.
- Fiscal Years: creación, actualización, apertura, cierre y archivo.
- Fiscal Periods: creación, búsqueda por fecha, apertura, cierre, lock, reapertura y archivo.
- Journal Entries: borradores, líneas, contabilización, reverso, void y búsqueda por número.
- Runtime Security: policies por permiso financiero.
- Accounting Hardening: invariantes contables y numeración endurecida.
- Integrated QA: flujo contable mínimo de punta a punta.
- SQL migration strategy: scripts versionados y runner básico.
- Docker smoke: script local para validar API, health y flujo contable.

## APIs disponibles

Ver `docs/api/financial-api-index.md` para el índice completo. Los grupos disponibles son:

- Chart of Accounts.
- Fiscal Years.
- Fiscal Periods.
- Journal Entries.
- Health.

## Permisos disponibles

Los permisos se agrupan en:

- `financial.chartofaccounts.*`
- `financial.fiscalyears.*`
- `financial.fiscalperiods.*`
- `financial.journalentries.*`
- `financial.*` para wildcard de desarrollo/administración según política.

## Configuraciones disponibles

- Tenant por defecto para ambiente local.
- Prefijo y padding de numeración contable.
- Flags de uso de adaptadores Portal.
- Cadena de conexión a `FinancieroDb`.
- JWT issuer/audience/secret por ambiente.
- Seq/logging y correlationId.

## Eventos auditables

- AccountCreated/Updated/Activated/Deactivated/Archived.
- FiscalYear/FiscalPeriod created/opened/closed/locked/reopened/archived.
- JournalEntryCreated/Updated/Posted/Reversed/Voided.

## Eventos outbox

- AccountChangedV1.
- FiscalPeriodClosedV1.
- AccountingEntryPostedV1.

## Limitaciones

- No incluye SRI ni facturación electrónica.
- No incluye frontend Angular.
- No incluye reporting financiero.
- No incluye Portal HTTP adapters productivos.
- El runner de migraciones es mínimo para Sprint 1.

## Próximos pasos

Recomendado: Sprint 2 — SRI & Electronic Invoicing Foundation, previa definición de certificados, catálogos SRI, secuencias tributarias, XML, firma, autorización, contingencia y almacenamiento documental.
