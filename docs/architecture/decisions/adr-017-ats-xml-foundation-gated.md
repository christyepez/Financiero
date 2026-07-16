# ADR-017 - ATS XML Foundation Gated

## Decisión

Crear preview XML ATS foundation, no oficial y gated por configuración, para validar estructura interna antes de revisión SRI/legal.

## Contexto

P1-P3 agregaron compras/anulados, mapping ATS y catálogos foundation. P4 permite construir un XML aproximado en memoria, con disclaimers y sin persistencia, envío ni cumplimiento oficial.

## Consecuencias

- Endpoints de readiness/generate-preview quedan protegidos por permisos existentes.
- `enabled=false` y `allowXmlPreview=false` son defaults seguros.
- XML solo puede retornarse si el caller tiene permiso manage, acknowledgements completos y configuración explícita.
- No se crea migración `014`.

## Restricciones

- No ATS oficial.
- No SRI submission.
- No XML real de contribuyentes.
- No XML en audit/logs.
- No storage propio ni Content/File upload.
