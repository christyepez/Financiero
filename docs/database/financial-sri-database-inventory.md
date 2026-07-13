# Financial SRI Database Inventory

## Tablas nuevas

- `financial.electronic_documents`
- `financial.electronic_document_lines`
- `financial.electronic_document_taxes`
- `financial.sri_document_sequences`
- `financial.sri_catalog_items`

## Migraciones

- `007_sri_electronic_documents.sql`
- `008_sri_sequences_catalogs.sql`
- `009_sri_signature_storage_metadata.sql`

## Reglas

- No SQL Server propio.
- Base lógica `FinancieroDb`.
- Schema `financial`.
- Secuencia tributaria separada de secuencia contable.
- Unicidad de access key.
- Unicidad de secuencial por tenant/tipo/ambiente/establecimiento/punto.
- Metadata de firma/storage en `financial.electronic_documents`.

## Sprint 2 P3 metadata

Migración `010_sri_ride_and_integration_metadata.sql` agrega `RideGeneratedAtUtc`, `RidePdfHash`, `StorageProvider`, `SriReceptionAttempts`, `SriAuthorizationAttempts` y `LastIntegrationCorrelationId`.

## Sprint 2 P4

P4 no agrega campos persistentes: readiness se calcula desde configuración runtime para evitar persistir secretos o estados sensibles.

## Catálogos seed mínimos

Tipos de comprobante, identificación, IVA e impuestos mínimos para foundation. Deben validarse contra normativa vigente antes de producción.
