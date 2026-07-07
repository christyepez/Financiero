# Financial API Contracts

All responses use:

```json
{ "data": {}, "error": null, "correlationId": "..." }
```

Errors use:

```json
{ "data": null, "error": { "code": "stable.error.code", "message": "Human message" }, "correlationId": "..." }
```

Development smoke can pass permissions with:

```http
X-Dev-Permissions: financial.*
```

Production must use JWT claims from Portal Security; `X-Dev-Permissions` is ignored.

## Chart of Accounts

| Method | Endpoint | Permission |
|---|---|---|
| GET | `/api/financial/accounts` | `financial.chartofaccounts.read` |
| GET | `/api/financial/accounts/{id}` | `financial.chartofaccounts.read` |
| GET | `/api/financial/accounts/by-code/{code}` | `financial.chartofaccounts.read` |
| GET | `/api/financial/accounts/tree` | `financial.chartofaccounts.read` |
| POST | `/api/financial/accounts` | `financial.chartofaccounts.create` |
| PUT | `/api/financial/accounts/{id}` | `financial.chartofaccounts.update` |
| POST | `/api/financial/accounts/{id}/activate` | `financial.chartofaccounts.activate` |
| POST | `/api/financial/accounts/{id}/deactivate` | `financial.chartofaccounts.deactivate` |
| POST | `/api/financial/accounts/{id}/archive` | `financial.chartofaccounts.archive` |

Create request:

```json
{ "code": "1.1", "name": "Caja", "type": "Asset", "level": 2, "parentAccountId": "...", "isMovementAccount": true }
```

## Fiscal Years and Periods

| Method | Endpoint | Permission |
|---|---|---|
| GET/POST/PUT | `/api/financial/fiscal-years` | read/create/update |
| POST | `/api/financial/fiscal-years/{id}/open` | `financial.fiscalyears.open` |
| POST | `/api/financial/fiscal-years/{id}/close` | `financial.fiscalyears.close` |
| POST | `/api/financial/fiscal-years/{id}/archive` | `financial.fiscalyears.archive` |
| GET/POST/PUT | `/api/financial/fiscal-periods` | read/create/update |
| GET | `/api/financial/fiscal-periods/open-by-date?date=2026-01-15` | `financial.fiscalperiods.read` |
| POST | `/api/financial/fiscal-periods/{id}/open|close|lock|reopen|archive` | matching `financial.fiscalperiods.*` |

## Journal Entries

| Method | Endpoint | Permission |
|---|---|---|
| GET | `/api/financial/journal-entries` | `financial.journalentries.read` |
| GET | `/api/financial/journal-entries/{id}` | `financial.journalentries.read` |
| GET | `/api/financial/journal-entries/by-number/{entryNumber}` | `financial.journalentries.read` |
| POST | `/api/financial/journal-entries` | `financial.journalentries.create` |
| PUT | `/api/financial/journal-entries/{id}` | `financial.journalentries.update` |
| POST/PUT/DELETE | `/api/financial/journal-entries/{id}/lines` | `financial.journalentries.update` |
| POST | `/api/financial/journal-entries/{id}/post` | `financial.journalentries.post` |
| POST | `/api/financial/journal-entries/{id}/reverse` | `financial.journalentries.reverse` |
| POST | `/api/financial/journal-entries/{id}/void` | `financial.journalentries.void` |

Create request:

```json
{ "postingDate": "2026-01-15", "source": "Manual", "reference": "REF", "description": "Entry", "lines": [] }
```

Line request:

```json
{ "accountId": "...", "description": "Debit", "debit": 10.00, "credit": 0.00 }
```

## Minimal curl

```bash
curl -H "X-Dev-Permissions: financial.*" http://localhost:8083/api/financial/accounts
```
