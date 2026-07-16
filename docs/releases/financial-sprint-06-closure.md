# Financial Sprint 6 Closure

Sprint 6 cierra la foundation Angular de Financiero como consumidor de PortalCorporativo.

## PRs

- #29 Angular Shell foundation.
- #30 Angular real data wiring.
- #31 Portal Shell contract foundation.
- #32 Controlled UI command foundation.

## Capacidades entregadas

- Angular standalone shell en `frontend/financiero-web`.
- Pantallas conectadas a backend local con estados loading/error/empty.
- `PortalShellContext` foundation para usuario, tenant, permisos, menú, feature flags y correlation.
- Providers standalone y portal-integrated placeholder.
- Dev headers bloqueados en producción.
- Comandos foundation controlados para compras y anulados, apagados por defecto.
- UX P5 con disclaimers, badges y mensajes sanitizados.

## Qué puede probarse

- Backend local en `http://localhost:8083`.
- Frontend con `pnpm run build` y `pnpm test`.
- Readiness SRI/Content/File, ATS readiness, catálogos, aprobaciones externas, compras y anulados foundation.
- Comandos foundation solo si development habilita flags y permisos.

## Bloqueos

- Login, roles, menú productivo y auth delegada real pertenecen a PortalCorporativo.
- SRI producción, SRI Test real, XAdES productivo, ATS oficial, RIDE legal final y XML preview completo siguen bloqueados.
- No se debe usar esta UI para cumplimiento tributario oficial.

## Seguridad

Sin login propio, sin token storage, sin certificados, sin XML real y sin identificaciones/access keys completas en UI. Los errores se sanitizan.

## Recomendación Sprint 7

Integrar contrato real de Angular Shell/Portal Security/Menu/Configuration y mantener todos los flujos productivos bajo gates explícitos.
