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

## Catálogos seed mínimos

Tipos de comprobante, identificación, IVA e impuestos mínimos para foundation. Deben validarse contra normativa vigente antes de producción.
