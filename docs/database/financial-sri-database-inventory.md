# Financial SRI Database Inventory

## Tablas nuevas

- `financial.electronic_documents`
- `financial.electronic_document_lines`
- `financial.electronic_document_taxes`
- `financial.electronic_document_references`
- `financial.electronic_document_debit_note_reasons`
- `financial.electronic_document_withholding_taxes`
- `financial.sri_document_sequences`
- `financial.sri_catalog_items`

## Migraciones

- `007_sri_electronic_documents.sql`
- `008_sri_sequences_catalogs.sql`
- `009_sri_signature_storage_metadata.sql`
- `010_sri_ride_and_integration_metadata.sql`
- `011_tax_documents_foundation.sql`
- `012_purchases_voided_documents_foundation.sql`

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

## Sprint 3 P1 tax documents

Migración `011_tax_documents_foundation.sql` agrega:

- referencias de documento modificado/sustento para NC, ND y retenciones;
- motivos de nota de débito;
- impuestos retenidos;
- índices por tenant/documento, número relacionado, periodo fiscal y tipo documental.

No almacena XML reales, certificados, secretos ni payloads de Portal Content/File.

## Sprint 3 P2 tax rules

No se crea migración `012`: los catálogos/reglas foundation se implementan en dominio y aplicación. No se agregan tablas, columnas, datos reales ni seeds oficiales.

## Catálogos seed mínimos

Tipos de comprobante, identificación, IVA e impuestos mínimos para foundation. Deben validarse contra normativa vigente antes de producción.
## Sprint 3 P3 RIDE/reporting

No se agrega migración 012. RIDE/reporting foundation consulta `financial.electronic_documents` y tablas relacionadas existentes (`lines`, `taxes`, `references`, `debit_note_reasons`, `withholding_taxes`). No se crea SQL Server propio ni base compartida.

## Sprint 3 P4 exports/ATS readiness

No se agrega migración 012. Exportaciones y ATS readiness se calculan en memoria sobre documentos electrónicos existentes. No se crean tablas de export, archivos persistidos ni SQL Server propio.

## Sprint 4 P1 Content/File readiness

No se agrega migración 012. RIDE y exports almacenables conservan metadata/hash usando campos existentes y storage ids externos. No se crean tablas de archivos, blobs, bases compartidas ni SQL Server propio.

## Sprint 4 P2 Content/File HTTP contract

No se agrega migración 012. El contrato HTTP, dry-run, token placeholder y validaciones no requieren persistencia nueva. No se guardan payloads, tokens, XML ni respuestas reales de Portal/SRI.

## Sprint 4 P3 RIDE/ATS design foundation

No se agrega migración 012. RIDE legal readiness y ATS official design son cálculos read-only sobre documentos existentes. No se guardan documentos finales, XML ATS, payloads ni secretos.

## Sprint 4 P4 tax/legal review gaps

No se agrega migración 012. Los gaps RIDE/ATS y checklist de aprobación son cálculos read-only. No se crean tablas de approvals, evidencias, archivos, XML ATS ni storage propio.

## Sprint 5 P1 purchases/voided foundation

Migración `012_purchases_voided_documents_foundation.sql` agrega tablas idempotentes para:

- `financial.purchase_tax_documents`
- `financial.purchase_tax_document_lines`
- `financial.purchase_taxes`
- `financial.purchase_support_document_references`
- `financial.voided_tax_documents`

Las tablas mantienen datos foundation de compras y anulados para readiness ATS. No almacenan XML, certificados, secretos, tokens, payloads SRI ni archivos. Se conserva SQL Server común local y base lógica separada `FinancieroDb`.

## Sprint 5 P2 ATS support mapping

No se crea migración `013`. El mapping de sustentos ATS y readiness por sección se calcula en aplicación sobre tablas existentes de compras/anulados y reglas foundation versionadas en código. No se crean tablas de mapping, XML ATS, evidencias, archivos ni storage propio.
