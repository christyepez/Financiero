# Sprint 1 P1 - Chart of Accounts Foundation

## Resultado

Plan de Cuentas implementado como primer vertical slice funcional de Accounting Core.

## Alcance

- Entidad `Account`.
- Casos de uso create, update, activate, deactivate, archive, get by id, get by code, search y tree.
- Endpoints `/api/financial/accounts`.
- Persistencia en `FinancieroDb`, schema `financial`, tabla `accounts`.
- Adaptación a Portal Security, Menu, Configuration, Audit y Outbox mediante metadata/contratos.

## Portal Security

- `financial.chartofaccounts.read`
- `financial.chartofaccounts.create`
- `financial.chartofaccounts.update`
- `financial.chartofaccounts.activate`
- `financial.chartofaccounts.deactivate`
- `financial.chartofaccounts.archive`
- `financial.chartofaccounts.manage`

## Portal Menu

- Módulo: `financiero`
- Grupo: `Contabilidad`
- Item: `Plan de cuentas`
- Ruta: `/financial/accounts`
- Permiso requerido: `financial.chartofaccounts.read`

## Portal Configuration

- `financial.accountCode.format`
- `financial.accountCode.maxLength`
- `financial.accountCode.separator`
- `financial.accountLevels.maxDepth`
- `financial.chartOfAccounts.allowManualCodes`
- `financial.chartOfAccounts.requireParentForLevelGreaterThanOne`

## Portal Audit

- `AccountCreated`
- `AccountUpdated`
- `AccountActivated`
- `AccountDeactivated`
- `AccountArchived`

## Outbox / Integration

- `FinancialAccountCreated`
- `FinancialAccountUpdated`
- `FinancialAccountStatusChanged`

## Exclusiones

No se implementa frontend, login, auditoría transversal propia, notificaciones propias, asientos contables, SRI ni facturación electrónica.
