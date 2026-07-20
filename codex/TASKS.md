# Roadmap Financiero

## Sprint 10 P1 — External Infra Remediation Kickoff

- [x] Crear intake formal para owners de Infra/Portal.
- [x] Crear plantillas sanitizadas de evidencia SQL/Gateway/Shell/Contrato.
- [x] Crear E2E acceptance gate.
- [x] Actualizar backlog externo con owners, gates y estados.
- [x] Actualizar risk register para evidencia/owner/gates.
- [x] Agregar soporte preflight para `-EvidenceOutputPath`, owner evidence y acceptance summary.
- [x] Fortalecer verificadores para Sprint 10 P1.
- [x] Mantener `BLOCKED_DEPENDENCY` y no producción.

## Sprint 9 P5 — Sprint Closure and External Infrastructure Decision

- [x] Crear cierre ejecutivo Sprint 9.
- [x] Crear evidencia final de infraestructura.
- [x] Crear matriz de decisión Sprint 10.
- [x] Crear backlog de dependencias externas.
- [x] Actualizar matriz de riesgos.
- [x] Crear release notes Sprint 9.
- [x] Fortalecer verificadores para cierre P5.
- [x] Mantener estado final `BLOCKED_DEPENDENCY` y no producción.

## Sprint 9 P4 — External Infrastructure Intervention

- [x] Crear paquete de intervención SQL común.
- [x] Crear paquete de intervención Portal Gateway/Shell.
- [x] Fortalecer preflight con rutas health configurables.
- [x] Crear evidencia P4 PASS/BLOCKED sanitizada.
- [x] Actualizar handoff/checklist de owners externos.
- [x] Mantener Financiero sin SQL Server propio ni capacidades Portal duplicadas.
- [x] Mantener `BLOCKED_DEPENDENCY` hasta evidencia real de SQL y Portal.

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
- [x] Sprint 5 P5: cierre Sprint 5, release notes, QA evidence, architecture snapshot y workflow foundation de aprobaciones externas ADR-018.
- [ ] Opción B gated: XAdES + SRI Test controlled send con certificado no productivo y custodia aprobada.
- [ ] Opción C gated: Portal Content/File real upload con contrato estable y aprobación.
- [ ] Opción D gated: RIDE legal final con revisión legal/tributaria.
- [ ] Opción E gated: ATS official XML foundation con schema/catálogos oficiales revisados.
- [ ] Integración con Angular Shell, Menu, Configuration y Security.

## Sprint 6 — Angular Shell

- [x] P1 Angular Shell foundation standalone en `frontend/financiero-web`.
- [x] Adapters placeholder para Portal Security, Menu y Notification sin login propio.
- [x] Interceptores frontend para correlation id, autorización delegada y sanitización de errores.
- [x] Clientes API read-only para SRI readiness, Content/File readiness, ATS, aprobaciones externas, catálogos, compras y anulados.
- [x] Documentar ADR-019 y guía frontend.
- [x] P2 cablear Angular Shell a datos reales sanitizados del backend local.
- [x] Desempaquetar `ApiResponse<T>` centralmente en frontend.
- [x] Agregar estados loading/error/empty y selector de período foundation.
- [x] Documentar ADR-020 y coordinación Sprint 6 P2.
- [x] P3 formalizar contrato Portal Shell foundation.
- [x] Agregar providers standalone y portal-integrated placeholder.
- [x] Conectar auth/menu/notificaciones/configuración/correlation al contexto Portal.
- [x] Documentar ADR-021 y contrato `window.__PORTAL_SHELL_CONTEXT__`.
- [x] P4 agregar comandos UI foundation controlados para compras/anulados.
- [x] Proteger comandos con feature flags y permiso `financial.electronicdocuments.manage`.
- [x] Mantener comandos apagados por defecto y documentar ADR-022.
- [x] P5 cerrar Sprint 6 con release notes, QA evidence, architecture snapshot y matriz Portal readiness.
- [x] Endurecer UX foundation para disclaimers, badges, errores, empty states y comandos deshabilitados.
- [x] Fortalecer `verify-frontend` contra token storage, artifacts, certificados, URLs productivas y docs faltantes.
- [ ] Reemplazar adapters placeholder por contrato real del Angular Shell de Portal.
- [ ] Publicar metadata de menú/permisos en Portal cuando el contrato esté congelado.

## Sprint 7 — Portal Shell integration

- [x] P1 endurecer contrato real Portal Shell `1.0`.
- [x] Agregar `contractVersion`, `source`, capabilities, warnings y validación de contexto.
- [x] Bloquear producción cuando falta contexto Portal real o versión soportada.
- [x] Filtrar menú por allow-list, permisos y feature flags.
- [x] Mantener delegated auth solo en memoria y sin token storage.
- [x] P2 agregar persistencia foundation de workflow de aprobaciones externas.
- [x] Crear migración `013_external_approval_workflow_foundation.sql`.
- [x] Agregar endpoints `/api/financial/external-approval-requests`.
- [x] Integrar UI Angular gated para requests y referencias metadata.
- [x] P3 agregar boundary Portal Content/File y Notification sin upload ni envío real.
- [x] Agregar endpoint `/api/financial/external-approval-requests/integration-readiness`.
- [x] Validar evidencia como metadata segura y notification intents foundation.
- [x] P4 agregar productization readiness read-only para compras y anulados.
- [x] Agregar endpoints `/api/financial/purchases/productization-readiness` y `/api/financial/voided-documents/productization-readiness`.
- [x] Mantener producción, SRI, ATS oficial, upload y notification send bloqueados.
- [x] P5 cerrar Sprint 7 con QA evidence, capability matrix, security checklist, release notes y roadmap Sprint 8.
- [x] Documentar blockers productivos y dependencias Portal/SQL/legal.
- [ ] Validar contrato end-to-end con PortalCorporativo real.

## Sprint 8 — Portal E2E and QA stabilization

- [x] P1 agregar checklist de validación E2E PortalCorporativo ↔ Financiero.
- [x] P1 agregar runbook local Portal + Financiero + SQL común.
- [x] P1 agregar endpoint read-only `/api/financial/portal-integration/readiness`.
- [x] P1 agregar verificador `verify-portal-e2e-contract.mjs`.
- [x] P1 agregar template de evidencia QA E2E.
- [x] P2 agregar script no invasivo `tools/validate-portal-financiero-e2e.ps1`.
- [x] P2 registrar evidencia de ejecución parcial E2E.
- [x] P2 documentar validación de SQL común y blockers reales.
- [x] P2 endurecer readiness endpoint/dashboard con SQL común, servicios esperados y drift indicators.
- [x] P3 estabilizar preflight QA con PASS/BLOCKED_DEPENDENCY/FAIL y exit codes 0/2/1.
- [x] P3 documentar plantilla de ambiente QA sin secretos.
- [x] P3 documentar troubleshooting health y diferencias dependency unavailable vs app failure.
- [x] P3 clasificar readiness Portal como BlockedDependency/FoundationOnly/ProductionBlocked.
- [x] P4 endurecer UX de aprobaciones externas con estados, blockers y próximos pasos seguros.
- [x] P4 mostrar ApprovedFoundation como no productivo y no sustituto de aprobación legal/tributaria.
- [x] P4 mostrar evidencia como Portal-owned metadata-only sin upload/download.
- [x] P4 mostrar notification intents foundation/no-send sin envío real.
- [x] P4 agregar evidencia QA textual de UX y fortalecer verificadores frontend.
- [x] P5 cerrar Sprint 8 con evidencia final E2E honesta.
- [x] P5 documentar matriz de decisión Sprint 9 y backlog productivo controlado.
- [x] P5 crear risk register y release notes Sprint 8.
- [x] P5 mantener resultado final `BLOCKED_DEPENDENCY` cuando SQL común/Portal no están disponibles.
- [ ] Ejecutar validación PASS con PortalShellContext real de PortalCorporativo.
- [ ] Validar Security/Menu/Configuration reales con Portal activo.
- [ ] Validar Gateway routing y SQL común local estable.
- [ ] Mantener bloqueados SRI Production, SRI Test real, ATS oficial, RIDE legal final, XAdES productivo, upload/download de evidencia y notification send.
- [ ] Decidir siguiente paquete entre Content/File real contract, Portal Notification contract o validación PASS con Portal real.

## Dependencias Portal Sprint 2

Catalog, Content/File, Reporting, Integration productiva, Angular Shell, IdP/OIDC, proveedor productivo de Notification y secret/certificate management. No suplir estas brechas duplicando plataforma.

## Sprint 9 — Real E2E infrastructure activation

- [x] P1 ejecutar preflight real contra SQL común y Portal Gateway/Shell.
- [x] P1 documentar evidencia `BLOCKED_DEPENDENCY` sin falso PASS.
- [x] P1 reforzar runbooks SQL/Portal con criterios PASS/BLOCKED_DEPENDENCY.
- [x] P1 validar que no existe SQL Server propio ni duplicación de Portal.
- [x] P2 agregar diagnósticos accionables al preflight (`-VerboseDiagnostics`, `-SuggestFixes`).
- [x] P2 documentar evidencia de dependencias SQL/Portal con códigos de causa.
- [x] P2 crear runbook de arranque SQL común + Portal Gateway/Shell.
- [x] P2 documentar overrides de ambiente sin secretos.
- [x] P3 capturar evidencia PASS/BLOCKED final sin falso PASS.
- [x] P3 crear checklist operativo para obtener PASS.
- [x] P3 crear handoff para owners de SQL/Portal.
- [x] P3 corregir clasificación HTTP 404 como `HTTP_STATUS_UNEXPECTED`.
- [ ] Levantar SQL común en `host.docker.internal:21433`.
- [ ] Levantar Portal Gateway/Shell y capturar PortalShellContext real.
- [ ] Reejecutar preflight hasta PASS.

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
