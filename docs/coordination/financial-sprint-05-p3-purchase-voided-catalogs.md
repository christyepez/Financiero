# Sprint 5 P3 - Purchase/Voided SRI Catalogs Foundation Hardening

Estado: implementado como catálogo foundation no oficial. No genera ATS XML, no activa SRI Test/Production y no reemplaza revisión tributaria.

## Alcance implementado

- Catálogo versionado `2026-07-sprint-5-p3-foundation`.
- Catálogos read-only para tipos de documento de compra, sustentos, documentos anulables, impuestos de compras e identificación de proveedor.
- Validación foundation con issues `foundation-only` y `requiresTaxReview`.
- Integración con ATS support mapping y ATS readiness mediante `catalogVersion`.
- Endpoints read-only protegidos por `financial.electronicdocuments.read`.
- Smoke y tests para catálogo, seguridad y sanitización.

## Fuera de alcance

- Catálogos oficiales finales.
- Descarga runtime de catálogos/XSD.
- ATS XML oficial.
- SRI producción o SRI Test real.
- Certificados/XML/datos reales.
- UI administrable de catálogos.

## Seguridad y Portal reuse

- Security: permisos runtime existentes.
- Audit: consultas de catálogos auditadas como `TaxCatalogsQueried`.
- Outbox: no se usa para consultas.
- SQL: sin migración `013`; catálogos en código foundation.
