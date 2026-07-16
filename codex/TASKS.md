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
- [x] Implementar P4 Secret Store, SRI Test dry-run/readiness, sanitización y Portal Content/File productive-readiness.
- [x] Implementar P5 wiring Key Vault, probe manual SRI Test, observabilidad sanitizada y contrato Content/File estabilizado.
- [x] Ejecutar P6 cierre Sprint 2 SRI readiness, release notes, QA evidence, checklist seguridad, runbook manual y backlog Sprint 3.
- [x] Agregar validación XML básica.
- [x] Agregar metadata de firma/storage y migración 009.
- [ ] Definir certificado/firma electrónica y estrategia XAdES.
- [ ] Definir ambientes SRI, RUC/empresa emisora y catálogos SRI.
- [ ] Diseñar secuencia de documentos tributarios.
- [ ] Diseñar XML schema, autorización SRI y contingencia.
- [ ] Integrar Audit/Outbox/Notification y Journal Entries.
- [x] Facturas, notas de crédito/débito y retenciones foundation.
- [ ] Liquidaciones de compra, guías, reglas tributarias completas y reporting.
- [ ] Diferir almacenamiento genérico a Content/File Portal.

## Sprint 3 — Integración SRI

- [x] Opción A: NC/ND/Retenciones foundation si no hay credenciales/certificado SRI no productivos.
- [ ] Opción B: Key Vault + XAdES real controlado + SRI Test manual si ya existe custodia segura aprobada fuera del repositorio.
- [ ] Opción C: adapter productivo Portal Content/File + RIDE hardening cuando Portal exponga contrato productivo estable.
- [ ] Requiere decisión explícita antes de activar envío real o firma productiva.

## Sprint 3 P1

- [x] Extender `ElectronicDocument` para Nota de Crédito `codDoc=04`.
- [x] Extender `ElectronicDocument` para Nota de Débito `codDoc=05`.
- [x] Extender `ElectronicDocument` para Retención `codDoc=07`.
- [x] Agregar referencias, motivos de débito e impuestos retenidos.
- [x] Agregar XML generators/validators foundation por tipo.
- [x] Agregar endpoints protegidos por permisos existentes.
- [x] Agregar migración `011_tax_documents_foundation.sql`.
- [x] Extender smoke SRI mock para NC/ND/retenciones.
- [x] Documentar ADR-006 y coordinación Sprint 3 P1.

## Sprint 3 P2

- [x] Implementar catálogo SRI foundation versionable.
- [x] Implementar provider `DevelopmentSriCatalogProvider`.
- [x] Endurecer reglas NC: documento relacionado, motivo, fecha, líneas y total > 0.
- [x] Endurecer reglas ND: documento relacionado, motivo/cargo y total > 0.
- [x] Endurecer reglas retenciones: periodo, taxCode, withholdingCode, base, porcentaje y cálculo.
- [x] Documentar redondeo decimal y tolerancia.
- [x] Reforzar XML validators por tipo documental.
- [x] Mantener Audit/Outbox y permisos runtime.
- [x] Documentar ADR-007 y coordinación Sprint 3 P2.

## Sprint 3 P3

- [x] Implementar RIDE foundation por factura, nota de crédito, nota de débito y retención.
- [x] Agregar preview RIDE sanitizado protegido por `financial.electronicdocuments.read`.
- [x] Implementar reporting tributario foundation: summary, documents, tax totals y withholding totals.
- [x] Extender smoke SRI mock con RIDE por tipo y reporting.
- [x] Documentar ADR-008 y coordinación Sprint 3 P3.

## Sprint 3 P4

- [x] Implementar export foundation JSON/CSV en memoria.
- [x] Implementar ATS readiness interno no oficial.
- [x] Implementar action queue y monthly summary.
- [x] Mantener sanitización de accessKey/identificación y exclusión de XML/secretos.
- [x] Documentar ADR-009 y coordinación Sprint 3 P4.

## Sprint 4 — Reporting financiero/fiscal

- [x] P1 Portal Content/File storage readiness: adapter productivo-ready, health/readiness, store-ride, export/store, smoke y documentación.
- [x] P2 contrato HTTP productivo-ready hacia Portal Content/File con dry-run, validator, auth placeholder y fake HTTP tests.
- [x] P3 RIDE legal layout foundation y ATS official design foundation con disclaimers y endpoints read-only.
- [x] P4 revisión tributaria/legal y cierre de gaps compras/anulados antes de RIDE/ATS oficial como gap/readiness management foundation.
- [x] P5 cierre Sprint 4, release notes, external review gate, QA evidence, architecture snapshot y backlog Sprint 5.
- [ ] RIDE legal final y ATS oficial; requiere revisión fiscal/manual.

## Sprint 5 — Frontend financiero

- [x] Opción A recomendada: compras y anulados foundation para reducir gaps ATS sin activar producción.
- [x] Sprint 5 P2: hardening de mapping de sustentos ATS, readiness por sección, endpoints read-only y documentación ADR-015.
- [x] Sprint 5 P3: catálogos foundation de compras/anulados, endpoints read-only, validación y documentación ADR-016.
- [x] Sprint 5 P4: ATS XML foundation gated, readiness, preview no oficial, smoke y documentación ADR-017.
- [ ] Opción B gated: XAdES + SRI Test controlled send con certificado no productivo y custodia aprobada.
- [ ] Opción C gated: Portal Content/File real upload con contrato estable y aprobación.
- [ ] Opción D gated: RIDE legal final con revisión legal/tributaria.
- [ ] Opción E gated: ATS official XML foundation con schema/catálogos oficiales revisados.
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
