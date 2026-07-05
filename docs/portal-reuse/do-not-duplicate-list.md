# Do Not Duplicate List

Codex no debe crear componentes duplicados para:

- Login.
- Administración de usuarios.
- Roles globales.
- Permisos globales.
- Menús dinámicos.
- Layout principal.
- Tema/look & feel.
- Auditoría visual.
- Centro de notificaciones.
- Envío genérico de correos.
- API Gateway.
- Catálogos generales.
- Configuración general.
- Gestión de contenido/archivos, si Content API permite almacenar documentos.
- Report shell/dashboard, si Reporting API permite extensiones.
- Outbox/Workers, si el portal ya tiene infraestructura extensible.

## Excepción

Solo se permite crear un componente si existe un ADR aprobado en:

```text
docs/accounting-tax-platform/adrs/
```
