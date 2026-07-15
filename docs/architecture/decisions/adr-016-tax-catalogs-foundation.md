# ADR-016 - Tax Catalogs Foundation

## Decisión

Crear catálogos foundation versionados en código para compras/anulados y exponerlos mediante endpoints read-only.

## Contexto

Sprint 5 P1/P2 dejó compras, anulados y mapping ATS como foundation. Para mejorar calidad de datos sin declarar cumplimiento oficial, P3 agrega catálogos explícitos con `IsFoundationOnly` y `RequiresTaxReview`.

## Consecuencias

- No se crea migración `013`.
- La versión de catálogo se propaga a mapping y readiness ATS.
- Códigos unsupported generan issues claros.
- Los catálogos siguen bloqueados para uso oficial hasta revisión externa.

## Restricciones

- No XML ATS oficial.
- No SRI producción/Test real.
- No secretos, certificados, XML ni datos reales.
- No duplicar Portal Security/Audit/Outbox/Content/File.
