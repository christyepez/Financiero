# ATS Official Design Foundation

Este documento describe una foundation técnica para ATS. No es un archivo ATS oficial y no certifica cumplimiento.

## Secciones

- Informante.
- Ventas.
- Compras.
- Retenciones.
- Anulados.
- Establecimientos.
- Resumen impuestos.

## Gaps conocidos

- Módulo de compras completo pendiente.
- Anulados/cancelaciones pendientes.
- Catálogos oficiales ATS pendientes de revisión.
- Layout XML oficial no implementado.

## Regla

No generar XML ATS oficial hasta completar revisión tributaria/legal, catálogos oficiales y evidencias de QA.

## Sprint 5 P1

Compras y anulados pasan de gap puramente estructural a foundation consultable cuando existen registros en el periodo:

- Compras: `PurchaseTaxDocument` con líneas, impuestos y referencias.
- Anulados: `VoidedTaxDocument`.
- Si no hay datos foundation del periodo, se mantienen los gaps `ats.purchases.module_missing` y `ats.voided_documents.model_missing`.
- Aunque existan datos, el ATS oficial sigue bloqueado por catálogos oficiales, XSD/layout final, revisión tributaria y evidencia SRI.
