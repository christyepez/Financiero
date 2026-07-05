# Portal Reuse Matrix - Base Inicial

Esta matriz parte del README público del repositorio PortalCorporativo. Codex debe actualizarla luego de inspeccionar el código.

| Capability | Portal Component | Strategy | Financial Action |
|---|---|---|---|
| Seguridad | Security API | REUSE/EXTEND | Agregar permisos financieros y usar token/claims del portal |
| Menús | Menu API | EXTEND | Registrar menú Financiero, Contabilidad, Facturación, SRI, Reportes |
| Integraciones | Integration API | EXTEND/ADAPT | Registrar conectores ERP/CRM/SRI y usar configuración central |
| Reportes | Reporting API | EXTEND | Exponer reportes financieros/tributarios como widgets/reportes |
| Auditoría | Audit API | ADAPT | Publicar eventos contables, tributarios y SRI |
| Notificaciones | Notification API | ADAPT | Notificar autorizaciones SRI, errores, vencimientos, aprobaciones |
| Contenido | Content API | ADAPT | Guardar/consultar XML, RIDE, anexos y evidencias |
| Catálogos | Catalog API | EXTEND | Usar para catálogos generales; crear tributarios específicos si aplica |
| Configuración | Configuration API | EXTEND | Parámetros SRI, secuenciales, establecimientos, puntos emisión |
| API Gateway | API Gateway | REUSE | Registrar APIs financieras detrás del gateway |
| SQL Outbox | SQL Outbox + Workers | EXTEND | Procesar eventos SRI/contables si la implementación es reutilizable |
| Docker Compose | docker-compose.yml | EXTEND | Agregar servicios financieros siguiendo patrón portal |
| Angular Shell | frontend/portal-angular | EXTEND | Agregar rutas y pantallas financieras dentro del portal |

## Regla

Si Codex decide `CREATE` donde esta matriz sugiere `REUSE`, `EXTEND` o `ADAPT`, debe crear un ADR.
