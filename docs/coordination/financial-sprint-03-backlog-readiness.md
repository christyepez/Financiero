# Sprint 3 Backlog Readiness

## Opción A - NC/ND/Retenciones Foundation

Objetivo: extender dominio tributario con notas de crédito, notas de débito y retenciones foundation, sin integración SRI real.

Riesgos: complejidad normativa y catálogos SRI.

Entrada: Sprint 2 cerrado, XML/factura foundation estable.

Salida: modelos, XML base, validaciones y tests.

## Opción B - Real Key Vault + XAdES + SRI Test Controlled Send

Objetivo: conectar secret store real, certificado de prueba fuera del repo, XAdES real y envío controlado a SRI Test.

Riesgos: credenciales, certificados, disponibilidad SRI Test y seguridad operacional.

Entrada: Key Vault, certificado test, URLs SRI Test y aprobación manual.

Salida: validación SRI Test con evidencia sanitizada.

## Opción C - Portal Content/File Productive Adapter + RIDE Hardening

Objetivo: cerrar almacenamiento documental con Portal Content/File y mejorar RIDE/PDF.

Riesgos: contrato Portal pendiente y payload handling.

Entrada: contrato final Content/File.

Salida: adapter productivo y RIDE más completo.

## Recomendación

Si aún no hay credenciales/certificado SRI Test, avanzar con Opción A. Si ya existen credenciales, certificado y Key Vault, avanzar con Opción B.

## Decisión pendiente

Confirmar disponibilidad de certificado de pruebas, secret store externo y endpoints SRI Test antes de elegir B.

## Prompt sugerido

Actúa como Backend Agent + Ecuador Tax/SRI Agent para Financiero. Ejecuta Sprint 3 según la opción seleccionada, manteniendo Portal-first, sin secretos reales, sin SRI producción y con PR hacia main.
