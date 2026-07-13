# Electronic Document Storage Strategy

## Ownership

Financiero no crea storage documental transversal. El almacenamiento productivo de XML/PDF debe delegarse a Portal Content/File.

## Puerto

`IElectronicDocumentStorageClient` define:

- `SaveUnsignedXmlAsync`.
- `SaveSignedXmlAsync`.
- `SaveAuthorizationXmlAsync`.
- `SaveRidePdfAsync`.

## Development

`DevelopmentElectronicDocumentStorageClient` registra metadata/hash con ids `dev://...` y no persiste archivos productivos.

## Productivo esperado

Provider `PortalContentFile` deberá enviar XML/PDF al Portal, recibir file id, hash, content type y metadata de retención.

## Retención e integridad

Se guardan hashes y storage ids en `financial.electronic_documents`. La política de retención se configura con:

- `financial.sri.storage.retainXml`.
- `financial.sri.storage.retainPdf`.
- `financial.sri.storage.container`.

## RIDE/PDF

RIDE queda placeholder; no se genera PDF final en P2.

## Sprint 2 P3

Se agrega `PortalContentFileStorageClient` como contrato hacia Portal Content/File. Si `financial.sri.storage.provider=PortalContentFile`, `financial.sri.storage.portalBaseUrl` es obligatorio. Financiero no crea storage documental propio; solo conserva ids, hash, provider y correlación.
