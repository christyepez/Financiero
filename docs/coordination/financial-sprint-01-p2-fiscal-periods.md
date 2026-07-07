# Sprint 1 P2 - Fiscal Periods Foundation

## Resultado

Fiscal Years y Fiscal Periods implementados como segundo vertical slice funcional de Accounting Core.

## Alcance

- Entidades `FiscalYear` y `FiscalPeriod`.
- Estados de año fiscal: `Draft`, `Open`, `Closed`, `Archived`.
- Estados de periodo fiscal: `Draft`, `Open`, `Closed`, `Locked`, `Archived`.
- Endpoints `/api/financial/fiscal-years` y `/api/financial/fiscal-periods`.
- Persistencia en `FinancieroDb`, schema `financial`, tablas `fiscal_years` y `fiscal_periods`.
- Adaptación a Portal Security, Menu, Configuration, Audit y Outbox mediante metadata/contratos.

## Portal Security

Fiscal Years:

- `financial.fiscalyears.read`
- `financial.fiscalyears.create`
- `financial.fiscalyears.update`
- `financial.fiscalyears.open`
- `financial.fiscalyears.close`
- `financial.fiscalyears.archive`
- `financial.fiscalyears.manage`

Fiscal Periods:

- `financial.fiscalperiods.read`
- `financial.fiscalperiods.create`
- `financial.fiscalperiods.update`
- `financial.fiscalperiods.open`
- `financial.fiscalperiods.close`
- `financial.fiscalperiods.lock`
- `financial.fiscalperiods.reopen`
- `financial.fiscalperiods.archive`
- `financial.fiscalperiods.manage`

## Portal Menu

- Módulo: `financiero`
- Grupo: `Contabilidad`
- Items: `Años fiscales`, `Periodos fiscales`
- Rutas: `/financial/fiscal-years`, `/financial/fiscal-periods`

## Portal Configuration

- `financial.fiscalYear.startMonth`
- `financial.fiscalYear.defaultPeriodType`
- `financial.fiscalPeriods.defaultCount`
- `financial.fiscalPeriods.allowReopen`
- `financial.fiscalPeriods.allowLock`
- `financial.fiscalPeriods.closeRequiresNoDraftEntries`

## Portal Audit

- `FiscalYearCreated`
- `FiscalYearUpdated`
- `FiscalYearOpened`
- `FiscalYearClosed`
- `FiscalYearArchived`
- `FiscalPeriodCreated`
- `FiscalPeriodUpdated`
- `FiscalPeriodOpened`
- `FiscalPeriodClosed`
- `FiscalPeriodLocked`
- `FiscalPeriodReopened`
- `FiscalPeriodArchived`

## Outbox / Integration

- `FiscalYearCreated`
- `FiscalYearStatusChanged`
- `FiscalPeriodCreated`
- `FiscalPeriodStatusChanged`

## Hardening P1

Plan de Cuentas ahora rechaza `Deactivate` y `Archive` de cuentas padre con hijos activos.

## Exclusiones

No se implementa frontend, Journal Entries, SRI ni facturación electrónica.
