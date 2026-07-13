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
- [x] Ejecutar P5 QA integrado, security smoke, concurrencia SQL y migraciones.
- [x] Ejecutar P6 documentación/cierre Sprint 1.
- [x] Reglas de partida doble, estados, cierre de período y pruebas de dominio base.
- [x] Extender permisos, Menu y Configuration para Plan de Cuentas mediante metadata.
- [x] Adaptar Audit y Outbox para Plan de Cuentas mediante contratos existentes.
- [x] Adaptar Audit y Outbox para Años/Periodos fiscales mediante contratos existentes.
- [x] Adaptar Audit y Outbox para Asientos contables mediante contratos existentes.
- [x] Aplicar autorización runtime por permisos `financial.*`.
- [x] Endurecer cuentas, períodos y asientos contra movimientos contables críticos.
- [x] Crear estrategia documentada de migraciones/versioning.
- [x] Crear scripts versionados `database/migrations/financial`.
- [x] Crear smoke script local.
- [x] Documentar contratos API financieros.

## Sprint 2 — Facturación y documentos tributarios

- [x] Implementar P1 SRI & Electronic Invoicing Foundation.
- [x] Modelar documentos electrónicos y factura foundation.
- [x] Implementar clave de acceso SRI con módulo 11.
- [x] Implementar XML base de factura.
- [x] Crear puertos firma/SRI y adapters development/mock.
- [x] Integrar permisos runtime, Audit/Outbox, migraciones y smoke SRI.
- [x] Implementar P2 contrato XAdES, SRI test/mock y storage strategy.
- [x] Implementar P3 XAdES adapter foundation, SRI SOAP Test contract, Portal Content/File contract, RIDE/PDF foundation y migración 010.
- [x] Agregar validación XML básica.
- [x] Agregar metadata de firma/storage y migración 009.
- [ ] Definir certificado/firma electrónica y estrategia XAdES.
- [ ] Definir ambientes SRI, RUC/empresa emisora y catálogos SRI.
- [ ] Diseñar secuencia de documentos tributarios.
- [ ] Diseñar XML schema, autorización SRI y contingencia.
- [ ] Integrar Audit/Outbox/Notification y Journal Entries.
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

## Sprint 1 P5

- [x] IntegratedAccountingFlowTests cubre flujo contable mínimo.
- [x] Security smoke cubre permisos positivos/negativos y `X-Dev-Permissions`.
- [x] Secuencia SQL usa transacción serializable con `UPDLOCK,HOLDLOCK`.
- [x] Readiness valida tablas core.
- [x] Runner básico usa `financial.schema_versions`.

## Sprint 1 P6

- [x] Crear closure document de Sprint 1.
- [x] Crear release notes.
- [x] Crear architecture snapshot.
- [x] Crear API index.
- [x] Crear permission matrix.
- [x] Crear database inventory.
- [x] Crear QA evidence.
- [x] Crear backlog readiness Sprint 2.
- [x] Actualizar README y TASKS.

## Deudas técnicas controladas

- Portal HTTP adapters productivos.
- Herramienta formal de migraciones.
- Load/concurrency test extendido.
- SRI/facturación.
- Firma XAdES productiva y clientes SOAP SRI.
- Almacenamiento XML/PDF con Portal Content/File.
- Frontend Angular.
- Reporting financiero.
