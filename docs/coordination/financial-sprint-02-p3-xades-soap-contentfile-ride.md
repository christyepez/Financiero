# Sprint 2 P3 - XAdES, SRI SOAP, Content/File y RIDE Foundation

P3 prepara contratos productivos controlados sin activar producción ni manejar certificados reales.

## Entregado

- Adapter foundation `XadesElectronicSignatureService` con proveedores placeholder seguros.
- Contratos SOAP SRI Test/Production con validación de URL y producción deshabilitada por defecto.
- Contrato `PortalContentFileStorageClient` para delegar XML/PDF a Portal Content/File.
- `DevelopmentRidePdfGenerator` para RIDE/PDF placeholder y metadata.
- Validación XML básica endurecida para estructura SRI de factura.
- Migración `010_sri_ride_and_integration_metadata.sql`.
- Endpoints `generate-ride` y `ride-metadata`.

## No incluido

- Firma XAdES productiva final.
- Lectura de certificados reales.
- Llamadas a SRI producción.
- RIDE productivo final.
- Storage documental propio.
