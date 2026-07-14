# Portal Content/File Adapter Contract

## Puerto consumidor

`IElectronicDocumentStorageClient` delega almacenamiento de:

- XML sin firmar.
- XML firmado.
- XML de autorización.
- RIDE/PDF.
- Preview HTML de RIDE.
- Exports fiscales JSON/CSV.
- Snapshot ATS readiness.

## Request esperado

`PortalContentFileRequest` contiene:

- `purpose`.
- `fileName`.
- `contentType`.
- `hash`.
- `size`.
- `container`.
- `tenantId`.
- `correlationId`.
- `metadata`.
- `includePayload`.
- `payloadBase64` opcional y redactado en logs.

## Metadata

`PortalContentFileMetadata` incluye `sourceSystem=Financiero`, documento, tipo, clave de acceso enmascarada, periodo, política de retención y valores extensibles.

## Configuración

- `financial.sri.storage.provider=Development|PortalContentFile|Disabled`.
- `financial.sri.storage.portalBaseUrl`.
- `financial.sri.storage.container`.
- `financial.sri.storage.timeoutSeconds`.
- `financial.sri.storage.sendPayloads=false`.
- `financial.sri.storage.maskPayloads=true`.
- `financial.sri.storage.allowProductionContentFilePayload=false`.
- `financial.sri.storage.retainXml=true`.
- `financial.sri.storage.retainPdf=false`.

## Política

Financiero no persiste archivos productivos. Portal Content/File debe ser el dueño de storage, retención, descarga, auditoría documental y lifecycle del archivo.
