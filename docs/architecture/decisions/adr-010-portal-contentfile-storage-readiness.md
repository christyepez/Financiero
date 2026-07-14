# ADR-010 - Portal Content/File Storage Readiness

## Estado

Aprobado para Sprint 4 P1.

## Contexto

Financiero genera XML, RIDE y exports fiscales, pero PortalCorporativo mantiene ownership de Content/File. Duplicar almacenamiento documental en Financiero rompería la arquitectura transversal.

## Decisión

Financiero mantiene un puerto `IElectronicDocumentStorageClient` y adapta metadata hacia Portal Content/File. En local se usa provider `Development`; en integración se configura `PortalContentFile`.

El adapter productivo-ready:

- exige `portalBaseUrl` cuando el provider es `PortalContentFile`;
- calcula hash del contenido;
- envía metadata funcional y técnica;
- evita payload por defecto;
- bloquea payloads en Production sin aprobación explícita;
- conserva solo storage id, hash, provider y propósito.

## Consecuencias

- No se crea storage documental propio.
- No se agregan tablas ni migraciones en P1.
- Los documentos financieros siguen apuntando a storage ids externos.
- El upload HTTP real queda diferido hasta contrato productivo Portal.

## Riesgos

- Contrato final de Portal Content/File puede cambiar.
- Retención legal de XML/RIDE requiere decisión fiscal.
- Payloads productivos requieren aprobación operativa y revisión de PII.
