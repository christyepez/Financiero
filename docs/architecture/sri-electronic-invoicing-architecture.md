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
