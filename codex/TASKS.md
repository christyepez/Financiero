# Roadmap Financiero

## Sprint 0 — Discovery

- [x] Clasificar capacidades Portal y dominio financiero.
- [x] Identificar bloqueos de Portal Sprint 2.

## Sprint 0.1 — Documentación base

- [x] Contexto, contratos de integración, arquitectura y ADR.
- [x] Crear AGENTS.md y README.md del repositorio.

## Sprint 1 — Contabilidad Core

- [x] Diseñar ChartOfAccounts, FiscalPeriods y JournalEntries; aprobar ADR-002.
- [x] Implementar P0 Bootstrap Financiero.
- [x] Implementar P1 Chart of Accounts.
- [x] Implementar P2 Fiscal Periods.
- [x] Implementar P3 Journal Entries.
- [x] Implementar P4 adaptadores Portal, seguridad runtime y hardening contable.
- [ ] Ejecutar P5 QA integrado y P6 documentación.
- [ ] Reglas de partida doble, estados, cierre de período y pruebas de dominio.
- [x] Extender permisos, Menu y Configuration para Plan de Cuentas mediante metadata.
- [x] Adaptar Audit y Outbox para Plan de Cuentas mediante contratos existentes.
- [x] Adaptar Audit y Outbox para Años/Periodos fiscales mediante contratos existentes.
- [x] Adaptar Audit y Outbox para Asientos contables mediante contratos existentes.
- [x] Aplicar autorización runtime por permisos `financial.*`.
- [x] Endurecer cuentas, períodos y asientos contra movimientos contables críticos.
- [x] Crear estrategia documentada de migraciones/versioning.

## Sprint 2 — Facturación y documentos tributarios

- [ ] Facturas, notas de crédito/débito, retenciones, liquidaciones y guías.
- [ ] Diferir almacenamiento genérico a Content/File Portal.

## Sprint 3 — Integración SRI

- [ ] XML, validación, firma, autorización, reintentos e Inbox.
- [ ] Requiere diseño de custodia segura e Integration API productiva.

## Sprint 4 — Reporting financiero/fiscal

- [ ] RIDE, ATS y reportes fiscales; requiere Reporting y Content/File.

## Sprint 5 — Frontend financiero

- [ ] Integración con Angular Shell, Menu, Configuration y Security.

## Dependencias Portal Sprint 2

Catalog, Content/File, Reporting, Integration productiva, Angular Shell, IdP/OIDC, proveedor productivo de Notification y secret/certificate management. No suplir estas brechas duplicando plataforma.

## Deuda técnica controlada P4

- Implementar adapters HTTP productivos hacia Portal cuando se congelen contratos de Security/Menu/Configuration/Audit/Outbox.
- Fortalecer secuencia con rowversion/UPDLOCK/sp_getapplock en P5 si las pruebas de concurrencia SQL lo requieren.
- Migrar desde `EnsureCreated` + SQL raw hacia migraciones versionadas antes de producción.
