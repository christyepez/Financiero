# Financial Sprint 6 Release Notes

## Summary

Sprint 6 entrega el shell Angular foundation de Financiero, conectado a backend real sanitizado y preparado para integrarse al Portal Shell.

## Included

- Dashboard foundation.
- SRI readiness y Content/File readiness.
- ATS readiness gated.
- Aprobaciones externas read-only.
- Catálogos tributarios foundation.
- Compras y documentos anulados foundation.
- Contrato `PortalShellContext`.
- Feature flags seguros.
- Comandos controlados de compras/anulados apagados por defecto.
- Hardening UX P5.

## Not included

- Login propio.
- Token storage.
- Producción SRI.
- SRI Test real.
- ATS oficial.
- RIDE legal final.
- XAdES productivo.
- XML preview completo.
- Aprobaciones productivas.

## Upgrade notes

Ejecutar:

```powershell
dotnet restore Financiero.sln
dotnet build Financiero.sln
DOTNET_ROLL_FORWARD=Major dotnet test Financiero.sln
cd frontend/financiero-web
pnpm install --frozen-lockfile
pnpm run build
pnpm test
```

Docker sigue usando SQL Server común externo y base lógica `FinancieroDb`.
