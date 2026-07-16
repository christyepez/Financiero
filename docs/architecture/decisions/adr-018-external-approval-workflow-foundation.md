# ADR-018 - External Approval Workflow Foundation

## Decisión

Crear un workflow foundation de aprobaciones externas calculado en código, read-only y advisory.

## Contexto

Sprint 5 dejó capacidades fiscales foundation para compras/anulados, mapping ATS, catálogos y preview XML ATS gated. Para avanzar a capacidades oficiales se requieren aprobaciones humanas, evidencia externa y gates de seguridad/operación.

## Consecuencias

- No se crea migración.
- No se guardan evidencias reales.
- No se activa producción.
- Los endpoints devuelven estado requerido y requisitos faltantes.

## Restricciones

No SRI real, no XML real, no certificados, no mutaciones, no workflow productivo irreversible.
