# SRI Electronic Invoicing Architecture

## Componentes

```text
Financiero API
  -> ElectronicDocumentsService
    -> ElectronicDocument domain
    -> SriAccessKeyGenerator
    -> IElectronicDocumentXmlGenerator
    -> IElectronicSignatureService
    -> ISriReceptionClient / ISriAuthorizationClient
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

P1 solo define `IElectronicSignatureService` e implementación Development. No lee certificados del repo ni base de datos. La integración productiva deberá usar Key Vault o secret store.

## Clientes SRI

P1 define `ISriReceptionClient` y `ISriAuthorizationClient` con dev/mock. No se invocan endpoints productivos ni SOAP real.

## Relación futura con contabilidad

`ElectronicDocument.RelatedJournalEntryId` permite vincular contabilización futura sin acoplar el flujo SRI al posting contable en P1.
