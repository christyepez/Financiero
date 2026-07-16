# ADR-019 - Angular Shell Portal Integration

## Estado

Aprobado para Sprint 6 P1.

## Contexto

Financiero requiere una UI inicial para operar como consumidor del Portal sin duplicar identidad, menú, notificaciones, auditoría ni storage documental.

## Decisión

Crear una aplicación Angular standalone en `frontend/financiero-web` con:

- Adapters reemplazables para Portal Security/Menu/Notification.
- Clientes HTTP read-only hacia APIs financieras existentes.
- Interceptores para correlation id, autorización delegada y sanitización.
- Configuración local sin secretos y sin headers dev por defecto.

## Consecuencias

- La UI puede evolucionar hacia el Angular Shell del Portal cuando el contrato esté congelado.
- Las pantallas P1 son foundation/readiness, no operación productiva.
- El frontend no crea persistencia, migraciones ni dominio nuevo.

## Restricciones

- No login propio.
- No almacenamiento local de tokens.
- No SRI producción.
- No XML/RIDE/ATS oficial completo.
- No certificados, secretos ni datos reales en repositorio.
