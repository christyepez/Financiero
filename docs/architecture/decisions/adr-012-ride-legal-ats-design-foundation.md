# ADR-012 - RIDE Legal and ATS Design Foundation

## Estado

Aprobado para Sprint 4 P3.

## Decisión

Financiero agrega una foundation para acercar RIDE y ATS a estructuras revisables, sin emitir documentos legalmente finales.

## RIDE

Se separan templates por tipo documental y se agrega readiness legal foundation. El HTML/PDF development conserva disclaimer explícito y no reemplaza un RIDE aprobado por revisión tributaria/legal.

## ATS

Se modela diseño oficial foundation mediante secciones, mappings, issues y unsupported reasons. No se genera XML ATS oficial.

## Consecuencias

- No hay migración.
- No hay storage propio.
- No hay nuevas llamadas SRI.
- Los endpoints nuevos son de lectura y requieren `financial.electronicdocuments.read`.

## Riesgos

- Cambios normativos o interpretación tributaria.
- Gaps de compras/anulados.
- Necesidad de revisión experta antes de cualquier uso productivo.
