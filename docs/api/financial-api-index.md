# Financial API Index

Todas las respuestas usan contrato `ApiResponse` y correlationId. Estado: implementado en Sprint 1 salvo que se indique lo contrario.

## Chart of Accounts

| Método | Ruta | Permiso requerido | Descripción | Estado | Ejemplo mínimo |
|---|---|---|---|---|---|
| POST | `/api/financial/accounts` | `financial.chartofaccounts.create` | Crear cuenta contable. | Implementado | `{ "code":"1.1", "name":"Caja", "type":"Asset", "level":2, "isMovementAccount":true }` |
| PUT | `/api/financial/accounts/{id}` | `financial.chartofaccounts.update` | Actualizar cuenta. | Implementado | `{ "name":"Caja General" }` |
| GET | `/api/financial/accounts` | `financial.chartofaccounts.read` | Buscar cuentas. | Implementado | `/api/financial/accounts?page=1&pageSize=20` |
| GET | `/api/financial/accounts/{id}` | `financial.chartofaccounts.read` | Obtener cuenta por id. | Implementado | n/a |
| GET | `/api/financial/accounts/by-code/{code}` | `financial.chartofaccounts.read` | Obtener cuenta por código. | Implementado | n/a |
| GET | `/api/financial/accounts/tree` | `financial.chartofaccounts.read` | Obtener árbol de cuentas. | Implementado | n/a |
| POST | `/api/financial/accounts/{id}/activate` | `financial.chartofaccounts.activate` | Activar cuenta. | Implementado | n/a |
| POST | `/api/financial/accounts/{id}/deactivate` | `financial.chartofaccounts.deactivate` | Desactivar cuenta. | Implementado | n/a |
| POST | `/api/financial/accounts/{id}/archive` | `financial.chartofaccounts.archive` | Archivar cuenta. | Implementado | n/a |

## Fiscal Years

| Método | Ruta | Permiso requerido | Descripción | Estado | Ejemplo mínimo |
|---|---|---|---|---|---|
| POST | `/api/financial/fiscal-years` | `financial.fiscalyears.create` | Crear año fiscal. | Implementado | `{ "year":2026, "startDate":"2026-01-01", "endDate":"2026-12-31" }` |
| PUT | `/api/financial/fiscal-years/{id}` | `financial.fiscalyears.update` | Actualizar año fiscal. | Implementado | `{ "code":"FY2026" }` |
| GET | `/api/financial/fiscal-years` | `financial.fiscalyears.read` | Buscar años fiscales. | Implementado | n/a |
| GET | `/api/financial/fiscal-years/{id}` | `financial.fiscalyears.read` | Obtener año fiscal. | Implementado | n/a |
| POST | `/api/financial/fiscal-years/{id}/open` | `financial.fiscalyears.open` | Abrir año fiscal. | Implementado | n/a |
| POST | `/api/financial/fiscal-years/{id}/close` | `financial.fiscalyears.close` | Cerrar año fiscal. | Implementado | n/a |
| POST | `/api/financial/fiscal-years/{id}/archive` | `financial.fiscalyears.archive` | Archivar año fiscal. | Implementado | n/a |

## Fiscal Periods

| Método | Ruta | Permiso requerido | Descripción | Estado | Ejemplo mínimo |
|---|---|---|---|---|---|
| POST | `/api/financial/fiscal-periods` | `financial.fiscalperiods.create` | Crear período fiscal. | Implementado | `{ "fiscalYearId":"...", "number":1, "startDate":"2026-01-01", "endDate":"2026-01-31" }` |
| PUT | `/api/financial/fiscal-periods/{id}` | `financial.fiscalperiods.update` | Actualizar período. | Implementado | n/a |
| GET | `/api/financial/fiscal-periods` | `financial.fiscalperiods.read` | Buscar períodos. | Implementado | n/a |
| GET | `/api/financial/fiscal-periods/{id}` | `financial.fiscalperiods.read` | Obtener período por id. | Implementado | n/a |
| GET | `/api/financial/fiscal-periods/open-by-date` | `financial.fiscalperiods.read` | Obtener período abierto por fecha. | Implementado | `?date=2026-01-15` |
| POST | `/api/financial/fiscal-periods/{id}/open` | `financial.fiscalperiods.open` | Abrir período. | Implementado | n/a |
| POST | `/api/financial/fiscal-periods/{id}/close` | `financial.fiscalperiods.close` | Cerrar período. | Implementado | n/a |
| POST | `/api/financial/fiscal-periods/{id}/lock` | `financial.fiscalperiods.lock` | Bloquear período. | Implementado | n/a |
| POST | `/api/financial/fiscal-periods/{id}/reopen` | `financial.fiscalperiods.reopen` | Reabrir período. | Implementado | n/a |
| POST | `/api/financial/fiscal-periods/{id}/archive` | `financial.fiscalperiods.archive` | Archivar período. | Implementado | n/a |

## Journal Entries

| Método | Ruta | Permiso requerido | Descripción | Estado | Ejemplo mínimo |
|---|---|---|---|---|---|
| POST | `/api/financial/journal-entries` | `financial.journalentries.create` | Crear asiento. | Implementado | `{ "postingDate":"2026-01-15", "source":"Manual", "reference":"REF", "description":"Entry", "lines":[] }` |
| PUT | `/api/financial/journal-entries/{id}` | `financial.journalentries.update` | Actualizar asiento draft. | Implementado | n/a |
| GET | `/api/financial/journal-entries` | `financial.journalentries.read` | Buscar asientos. | Implementado | n/a |
| GET | `/api/financial/journal-entries/{id}` | `financial.journalentries.read` | Obtener asiento por id. | Implementado | n/a |
| GET | `/api/financial/journal-entries/by-number/{entryNumber}` | `financial.journalentries.read` | Obtener asiento por número. | Implementado | n/a |
| POST | `/api/financial/journal-entries/{id}/lines` | `financial.journalentries.update` | Agregar línea. | Implementado | `{ "accountId":"...", "debit":10, "credit":0 }` |
| PUT | `/api/financial/journal-entries/{id}/lines/{lineId}` | `financial.journalentries.update` | Actualizar línea. | Implementado | n/a |
| DELETE | `/api/financial/journal-entries/{id}/lines/{lineId}` | `financial.journalentries.update` | Eliminar línea. | Implementado | n/a |
| POST | `/api/financial/journal-entries/{id}/post` | `financial.journalentries.post` | Contabilizar asiento. | Implementado | n/a |
| POST | `/api/financial/journal-entries/{id}/reverse` | `financial.journalentries.reverse` | Reversar asiento. | Implementado | n/a |
| POST | `/api/financial/journal-entries/{id}/void` | `financial.journalentries.void` | Anular draft. | Implementado | n/a |

## Health

| Método | Ruta | Permiso requerido | Descripción | Estado | Ejemplo mínimo |
|---|---|---|---|---|---|
| GET | `/health` | Anónimo | Health general. | Implementado | n/a |
| GET | `/health/live` | Anónimo | Liveness. | Implementado | n/a |
| GET | `/health/ready` | Anónimo | Readiness DB/tablas. | Implementado | n/a |
