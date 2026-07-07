# Sprint 1 P4 — Portal Adapters / Runtime Security / Accounting Hardening

## Resultado

P4 cierra el hardening mínimo del Accounting Core y prepara integración runtime con Portal sin duplicar capacidades transversales.

## Seguridad runtime

- Endpoints protegidos por permisos `financial.*`.
- Claims aceptadas: `permission`, `permissions`, `scope`, `roles` y roles estándar.
- Development permite `X-Dev-Permissions` con permisos separados por coma.
- Production ignora `X-Dev-Permissions`.
- Portal Security mantiene ownership de usuarios, roles y permisos.

## Hardening aplicado

- Cuentas con hijos activos no se desactivan/archivan.
- Cuentas usadas en `JournalEntry` Posted no se desactivan/archivan.
- Cuentas usadas en Posted no cambian código ni pasan de movimiento a resumen.
- FiscalPeriod no cierra con Draft JournalEntries si `financial.fiscalPeriods.closeRequiresNoDraftEntries=true`.
- FiscalPeriod no bloquea con Draft JournalEntries.
- FiscalPeriod no se archiva con Posted/Reversed JournalEntries.
- FiscalYear no se archiva con Posted/Reversed JournalEntries.
- JournalEntry void respeta `financial.journalEntries.allowVoidDraft`.
- JournalEntry numbering usa `financial.journalEntries.numbering.prefix` y `padding`.

## Portal adapters

Se conservan `DevelopmentPortalAdapters` para local. Las interfaces `IPortalSecurityClient`, `IPortalConfigurationClient`, `IPortalMenuRegistrationClient`, `IPortalAuditClient` e `IPortalOutboxClient` quedan estables. Los adapters HTTP productivos se difieren hasta congelar contratos reales de Portal.

## Validación esperada

- `dotnet build Financiero.sln`
- `$env:DOTNET_ROLL_FORWARD='Major'; dotnet test Financiero.sln`
- `docker compose config --quiet`
- `docker compose up -d --build`
- `GET http://localhost:8083/health`

## Riesgos

- Secuencia sigue siendo gap-tolerant; P5 debe validar concurrencia SQL y decidir rowversion/UPDLOCK/sp_getapplock.
- Migraciones versionadas están documentadas; el runtime local conserva `EnsureCreated` + SQL raw.
- Adapters HTTP productivos dependen de contratos de Portal.
