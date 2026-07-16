# ADR-020 - Angular Real Data Wiring

## Estado

Aprobado para Sprint 6 P2.

## Contexto

Sprint 6 P1 creó el shell Angular foundation. Para P2 se requiere consumir datos reales del backend local sin habilitar operaciones productivas ni duplicar capacidades del Portal.

## Decisión

- El frontend desempaqueta `ApiResponse<T>` centralmente en `ApiService`.
- Las pantallas usan endpoints read-only existentes y estados loading/error/empty.
- Los datos sensibles se tratan defensivamente en frontend aunque el backend ya entregue respuestas sanitizadas.
- Los adapters de Portal Security/Menu/Notification siguen como puntos de integración, sin identidad propia.

## Consecuencias

- La UI queda conectada al backend real local.
- No se agregan migraciones ni cambios de dominio.
- Las acciones mutables quedan diferidas a sprints gated.

## Restricciones

- No login propio.
- No token storage.
- No XML completo ni accessKey completa.
- No SRI producción ni SRI Test real desde UI.
- No aprobaciones mutables ni upload de evidencia.
