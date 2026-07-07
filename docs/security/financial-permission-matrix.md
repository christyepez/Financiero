# Financial Permission Matrix

## Modelo

Financiero consume permisos emitidos por Portal Security. En Development se puede usar `X-Dev-Permissions`; en Production se exigen claims JWT. El wildcard `financial.*` aplica para administración/smoke según ambiente y no reemplaza permisos productivos de menor privilegio.

## Chart of Accounts

| Permiso | Operación | Endpoint asociado |
|---|---|---|
| `financial.chartofaccounts.read` | Leer cuentas | `GET /api/financial/accounts*` |
| `financial.chartofaccounts.create` | Crear cuenta | `POST /api/financial/accounts` |
| `financial.chartofaccounts.update` | Actualizar cuenta | `PUT /api/financial/accounts/{id}` |
| `financial.chartofaccounts.activate` | Activar cuenta | `POST /api/financial/accounts/{id}/activate` |
| `financial.chartofaccounts.deactivate` | Desactivar cuenta | `POST /api/financial/accounts/{id}/deactivate` |
| `financial.chartofaccounts.archive` | Archivar cuenta | `POST /api/financial/accounts/{id}/archive` |
| `financial.chartofaccounts.manage` | Administración amplia | Equivalente operacional por política |

## Fiscal Years

| Permiso | Operación | Endpoint asociado |
|---|---|---|
| `financial.fiscalyears.read` | Leer años | `GET /api/financial/fiscal-years*` |
| `financial.fiscalyears.create` | Crear año | `POST /api/financial/fiscal-years` |
| `financial.fiscalyears.update` | Actualizar año | `PUT /api/financial/fiscal-years/{id}` |
| `financial.fiscalyears.open` | Abrir año | `POST /api/financial/fiscal-years/{id}/open` |
| `financial.fiscalyears.close` | Cerrar año | `POST /api/financial/fiscal-years/{id}/close` |
| `financial.fiscalyears.archive` | Archivar año | `POST /api/financial/fiscal-years/{id}/archive` |
| `financial.fiscalyears.manage` | Administración amplia | Equivalente operacional por política |

## Fiscal Periods

| Permiso | Operación | Endpoint asociado |
|---|---|---|
| `financial.fiscalperiods.read` | Leer períodos | `GET /api/financial/fiscal-periods*` |
| `financial.fiscalperiods.create` | Crear período | `POST /api/financial/fiscal-periods` |
| `financial.fiscalperiods.update` | Actualizar período | `PUT /api/financial/fiscal-periods/{id}` |
| `financial.fiscalperiods.open` | Abrir período | `POST /api/financial/fiscal-periods/{id}/open` |
| `financial.fiscalperiods.close` | Cerrar período | `POST /api/financial/fiscal-periods/{id}/close` |
| `financial.fiscalperiods.lock` | Bloquear período | `POST /api/financial/fiscal-periods/{id}/lock` |
| `financial.fiscalperiods.reopen` | Reabrir período | `POST /api/financial/fiscal-periods/{id}/reopen` |
| `financial.fiscalperiods.archive` | Archivar período | `POST /api/financial/fiscal-periods/{id}/archive` |
| `financial.fiscalperiods.manage` | Administración amplia | Equivalente operacional por política |

## Journal Entries

| Permiso | Operación | Endpoint asociado |
|---|---|---|
| `financial.journalentries.read` | Leer asientos | `GET /api/financial/journal-entries*` |
| `financial.journalentries.create` | Crear asiento | `POST /api/financial/journal-entries` |
| `financial.journalentries.update` | Actualizar asiento/líneas | `PUT /api/financial/journal-entries/{id}`, líneas |
| `financial.journalentries.post` | Contabilizar | `POST /api/financial/journal-entries/{id}/post` |
| `financial.journalentries.reverse` | Reversar | `POST /api/financial/journal-entries/{id}/reverse` |
| `financial.journalentries.void` | Void draft | `POST /api/financial/journal-entries/{id}/void` |
| `financial.journalentries.manage` | Administración amplia | Equivalente operacional por política |

## Ambientes

- Development: `X-Dev-Permissions: financial.*` o permisos específicos para smoke local.
- Production: claims JWT `permission`, `permissions`, `scope` o `roles`, emitidos por Portal/IdP.

## Deuda controlada

La integración productiva con Portal Security real queda pendiente hasta cerrar contratos HTTP/claims finales. Financiero no debe almacenar identidades ni roles locales.
