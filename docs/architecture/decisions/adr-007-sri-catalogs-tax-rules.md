# ADR-007 - SRI Catalogs and Tax Rules Foundation

## Estado

Aprobado para Sprint 3 P2.

## Contexto

Sprint 3 P1 agregó foundation para Nota de Crédito, Nota de Débito y Retenciones reutilizando `ElectronicDocument`. El siguiente riesgo era permitir documentos formalmente incompletos o tributariamente inconsistentes antes de llegar a XSD/SRI real.

## Decisión

Se implementan catálogos internos foundation y validadores de reglas en dominio/aplicación:

- `SriCatalogService`.
- `DevelopmentSriCatalogProvider`.
- `SriTaxRuleValidator`.
- `TaxCalculationValidator`.
- `MoneyRoundingPolicy`.

Los catálogos son versionables en código y no se descargan automáticamente. Se evita migración nueva porque no hay persistencia adicional requerida.

## Reglas cerradas

- Solo emisión normal en foundation.
- Ambiente Production sigue bloqueado por configuración operativa.
- Periodo fiscal acepta `MM/YYYY` o `YYYY-MM`.
- Documento relacionado/sustento usa formato `###-###-#########`.
- Nota de crédito y débito deben tener total mayor a cero.
- Retención debe tener base > 0, porcentaje > 0, valor retenido >= 0, valor retenido <= base y cálculo consistente con tolerancia `0.01`.
- XML debe mantener root/codDoc/claveAcceso consistentes por tipo.

## Consecuencias

- Se reduce riesgo de generar XML foundation inválido.
- Las reglas quedan probadas sin acoplarse a servicios externos.
- Persistencia y Portal reuse permanecen intactos.

## Diferido

- Catálogos oficiales completos.
- XSD oficial.
- SRI Test send real.
- XAdES productivo.
- Motor fiscal/contable automático.
