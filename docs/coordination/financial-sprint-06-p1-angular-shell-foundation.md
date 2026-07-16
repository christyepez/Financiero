# Sprint 6 P1 - Angular Shell Integration Foundation

Estado: implementado como foundation frontend read-only.

## Alcance

- Crear `frontend/financiero-web` como Angular standalone.
- Integrar adapters placeholder para Portal Security, Menu y Notification.
- Consumir APIs financieras existentes de readiness SRI, Content/File, ATS, aprobaciones externas, catálogos, compras y anulados.
- Mantener UI read-only y sanitizada.

## No alcance

- No login propio.
- No gestión local de usuarios, roles ni permisos.
- No envío SRI test/producción desde UI.
- No RIDE/ATS oficial.
- No preview XML completo ni claves de acceso completas.
- No duplicar Portal Security, Menu, Notification, Audit, Content/File ni Gateway.

## Criterios de aceptación

- `npm run build` compila la aplicación.
- `npm test` valida estructura y ausencia de patrones sensibles.
- Interceptores preparan correlation id, autorización delegada y sanitización de errores.
- La configuración `enableDevHeaders` queda desactivada por defecto.
- La UI consume contratos existentes sin mutar dominio financiero.

## Riesgos

- Los contratos de Portal Angular Shell aún no están congelados; los adapters son reemplazables.
- Las respuestas backend pueden variar entre endpoints; la UI tolera campos opcionales.
- La activación de headers dev debe seguir limitada a entorno local controlado.
