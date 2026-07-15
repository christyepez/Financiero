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

## Sprint 5 P2

El mapping de sustentos ATS se endurece como evaluación read-only:

- Compras: factura, nota de crédito y nota de débito foundation se mapean a sección `Purchases`.
- Liquidación de compra queda como placeholder sujeto a revisión tributaria.
- Retenciones relacionadas se declaran como sección futura `Withholdings`.
- Anulados foundation se mapean a `VoidedDocuments`.
- `ats-readiness`, `ats-official-design` y gaps agregan detalle de secciones con campos faltantes o unsupported.

La salida sigue siendo no oficial: no se genera XML ATS, no se valida XSD oficial y no se certifica cumplimiento.
