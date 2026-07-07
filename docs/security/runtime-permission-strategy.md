# Runtime Permission Strategy

Financiero no implementa login ni Security API. La fuente de verdad es Portal Security.

## Producción

Los permisos deben llegar en JWT emitido/validado por Portal mediante claims:

- `permission`
- `permissions`
- `scope`
- `roles`
- roles estándar de ASP.NET

El header `X-Dev-Permissions` se ignora en Production.

## Development

Para smoke tests locales se permite:

```http
X-Dev-Permissions: financial.chartofaccounts.read,financial.journalentries.post
```

También se acepta `financial.*` para pruebas manuales amplias.

## Permisos aplicados

- Accounts: `financial.chartofaccounts.read|create|update|activate|deactivate|archive`.
- Fiscal years: `financial.fiscalyears.read|create|update|open|close|archive`.
- Fiscal periods: `financial.fiscalperiods.read|create|update|open|close|lock|reopen|archive`.
- Journal entries: `financial.journalentries.read|create|update|post|reverse|void`.
