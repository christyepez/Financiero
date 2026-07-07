# Sprint 2 P2 — XAdES / SRI Test Contract / XML-PDF Storage Strategy

## Alcance

P2 fortalece la base de documentos electrónicos sin activar producción: contrato de firma XAdES, contrato SRI test/mock, validación XML básica, metadata de firma/storage y estrategia de almacenamiento delegada a Portal Content/File.

## Implementado

- `SignatureProviderType`: Development, Disabled, External, LocalCertificatePlaceholder.
- `SignatureResult` con provider, alias, fecha, digest y modo.
- Rechazo explícito de provider Development en Production.
- Rechazo explícito de LocalCertificatePlaceholder y External si no hay adapter real.
- Contratos SRI request/response normalizados.
- `SriResponseStatus`: Received, Returned, Processing, Authorized, Rejected, NotFound, Error.
- `IElectronicDocumentXmlValidator` con validación básica de factura.
- `IElectronicDocumentStorageClient` con adapter development/placeholder.
- Endpoints `validate-xml`, `status`, `storage-metadata`.
- Migración `009_sri_signature_storage_metadata.sql`.
- Smoke SRI extendido.

## No implementado

- Firma XAdES productiva.
- Lectura de certificados reales.
- Envío a SRI producción.
- SOAP real.
- RIDE/PDF productivo.
- Storage documental propio.

## Resultado

Financiero queda preparado para integrar firma y SRI test reales sin subir certificados ni duplicar Portal Content/File.
