# Sprint 5 P1 - Purchases and Voided Documents Foundation

Estado: implementado como foundation técnica para readiness ATS. No genera ATS oficial, no usa XML real, no llama SRI producción y no duplica capacidades de PortalCorporativo.

## Alcance implementado

- Modelo de compras tributarias: cabecera, líneas, impuestos y referencias de sustento.
- Modelo de documentos anulados foundation.
- Validaciones mínimas: periodo `YYYY-MM`, número `###-###-#########`, fecha no futura, catálogos foundation, totales e impuestos con tolerancia.
- Endpoints backend para crear, validar, consultar compras y registrar/consultar anulados.
- Migración idempotente `012_purchases_voided_documents_foundation.sql`.
- Integración ATS readiness/design/gaps para reducir faltantes de compras y anulados cuando existen datos foundation.
- Auditoría y Outbox reutilizados desde Portal, sin crear auditoría propia.

## Fuera de alcance

- XML ATS oficial.
- Sustentos, catálogos y validaciones tributarias finales.
- Envío SRI producción.
- RIDE legal final.
- Frontend Angular.
- Storage documental propio.

## Seguridad y Portal reuse

- Permisos usados: `financial.electronicdocuments.read` y `financial.electronicdocuments.manage`.
- Identificación de proveedor, access key y autorización se devuelven enmascaradas.
- No se persisten certificados, secretos, tokens, XML ni payloads reales.
- Se mantiene SQL Server común local con base lógica `FinancieroDb`.

## QA

- Tests de dominio/aplicación/API cubren validación, duplicados, enmascaramiento, permisos y migración idempotente.
- Smoke SRI mock agrega compra/anulado sintético antes de verificar ATS readiness y gaps.
