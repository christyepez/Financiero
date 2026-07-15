# ADR-015 - ATS Support Document Mapping Foundation

## Decisión

Agregar una capa read-only de mapeo de sustentos y secciones ATS sobre los modelos foundation existentes de compras y documentos anulados.

## Contexto

Sprint 5 P1 creó persistencia foundation para compras y anulados. Para avanzar hacia ATS sin duplicar capacidades del Portal ni generar XML oficial, Sprint 5 P2 necesita declarar qué datos existen, qué campos faltan y qué secciones siguen sujetas a revisión tributaria.

## Consecuencias

- `AtsSupportMappingService` centraliza reglas foundation y readiness por sección.
- `TaxExportService` y `AtsOfficialGapAnalysisService` incorporan detalle de mapping sin persistir resultados.
- Se agregan endpoints read-only para consumidores internos y QA.
- No se agrega migración `013`; el mapeo se calcula desde datos existentes.

## Restricciones

- No XML ATS oficial.
- No datos reales, certificados ni secretos.
- No storage documental propio.
- No SQL Server propio.
- No mutaciones ni aprobaciones productivas desde estos endpoints.
