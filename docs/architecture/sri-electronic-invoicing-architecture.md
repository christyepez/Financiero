# SRI Electronic Invoicing Architecture

## Componentes

```text
Financiero API
  -> ElectronicDocumentsService
    -> ElectronicDocument domain
    -> SriAccessKeyGenerator
    -> IElectronicDocumentXmlGenerator
    -> IElectronicSignatureService
    -> IElectronicDocumentXmlValidator
    -> ISriReceptionClient / ISriAuthorizationClient
    -> IElectronicDocumentStorageClient
    -> IPortalAuditClient / IPortalOutboxClient
    -> IElectronicDocumentRepository
      -> FinancieroDb / schema financial
```

## Portal reuse

- Security: permisos `financial.electronicdocuments.*`.
- Configuration: claves `financial.sri.*`.
- Audit: eventos de documentos electrónicos por puerto.
- Outbox: eventos de integración por puerto.
- Gateway/health/logging/correlationId: reutilizados.
- SQL común: se usa `FinancieroDb`; no SQL Server propio.

## Secuencias

`financial.sri_document_sequences` reserva secuenciales por tenant, tipo, ambiente, establecimiento y punto de emisión. No se mezcla con `accounting_sequences`.

Sprint 3 P1 reutiliza la misma estrategia para:

- Factura, `codDoc=01`.
- Nota de Crédito, `codDoc=04`.
- Nota de Débito, `codDoc=05`.
- Comprobante de Retención, `codDoc=07`.

## Firma

P2 agrega providers Development, Disabled, External y LocalCertificatePlaceholder. Development se bloquea en Production; providers reales fallan explícitamente hasta tener adapter seguro.

## Clientes SRI

P2 normaliza request/response y estados SRI. No se invocan endpoints productivos ni SOAP real.

## Storage

P2 agrega puerto `IElectronicDocumentStorageClient`. El adapter development registra hashes/metadata; producción debe delegar XML/PDF a Portal Content/File.

P3 agrega foundation para XAdES, SOAP Test, Portal Content/File y RIDE. Todos los adapters productivos quedan deshabilitados por configuración segura; el flujo local continúa con Development/Mock.

P4 agrega Secret Store, sanitización y readiness SRI. Los adapters productivos siguen bloqueados por gates explícitos.

P5 fortalece Key Vault wiring, probe manual SRI Test y observabilidad sanitizada. Production y envío real permanecen bloqueados.

## Relación futura con contabilidad

`ElectronicDocument.RelatedJournalEntryId` permite vincular contabilización futura sin acoplar el flujo SRI al posting contable en P1.

## Documentos tributarios Sprint 3 P1

`ElectronicDocument` se mantiene como agregado raíz. Las notas de crédito, notas de débito y retenciones agregan hijos persistidos para la información que no pertenece a factura:

- `electronic_document_references`: documento modificado o sustento.
- `electronic_document_debit_note_reasons`: motivos y valores de nota de débito.
- `electronic_document_withholding_taxes`: impuestos retenidos por periodo fiscal.

Los generadores y validadores XML son foundation. Antes de producción deben compararse contra XSD y reglas SRI vigentes.

## Catálogos y reglas Sprint 3 P2

P2 agrega catálogos internos foundation versionables y validadores tributarios:

- `SriCatalogService` y `DevelopmentSriCatalogProvider`.
- `SriTaxRuleValidator`.
- `TaxCalculationValidator`.
- `MoneyRoundingPolicy`.

La política de redondeo usa `decimal`, 2 posiciones, `AwayFromZero` y tolerancia `0.01`. No reemplaza un motor fiscal completo ni catálogos oficiales revisados.
