# ADR-004 - SRI XAdES, SOAP Test, Portal Content/File y RIDE Foundation

## Estado

Aprobado para Sprint 2 P3.

## Decisión

Financiero define puertos propios para firma, SRI SOAP, almacenamiento y RIDE, pero delega capacidades transversales al Portal cuando existan adapters productivos.

- Firma real: XAdES queda deshabilitado hasta contar con certificate provider seguro.
- Certificados: prohibido versionar o almacenar secretos/certificados en repo.
- SRI SOAP: Test/Production requieren URLs por configuración; Production requiere `financial.sri.allowProduction=true` futuro.
- Storage: XML/PDF productivos pertenecen a Portal Content/File; Financiero solo conserva metadata/hash.
- RIDE: P3 entrega generador Development para smoke y contrato futuro.

## Consecuencias

Se desbloquean pruebas de contrato sin comprometer secretos ni llamar servicios externos reales.
