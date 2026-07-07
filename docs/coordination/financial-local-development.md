# Desarrollo local de Financiero

## Requisitos

.NET 8 SDK y Docker Compose v2. PortalCorporativo provee el SQL Server local único y no se duplica desde este Compose.

## Build y pruebas

```powershell
dotnet restore Financiero.sln
dotnet build Financiero.sln --no-restore
dotnet test Financiero.sln
```

Si el host no tiene runtime .NET 8, ejecutar pruebas con SDK Docker:

```powershell
docker run --rm -v "${PWD}:/src" -w /src mcr.microsoft.com/dotnet/sdk:8.0 dotnet test Financiero.sln
```

## Docker

Levantar primero el SQL Server base de PortalCorporativo:

```powershell
cd ..\PortalCorporativo
docker compose up -d sqlserver
```

Luego copiar `.env.example` a `.env` en Financiero, usar el mismo `SQLSERVER_SA_PASSWORD` de PortalCorporativo y ejecutar:

```powershell
docker compose up -d --build financial-api
```

API: `http://localhost:8081`; health: `/health`, `/health/live`, `/health/ready`. Detener Financiero con `docker compose down`; no usar `docker compose down -v` sobre PortalCorporativo salvo que se quiera eliminar el volumen local compartido de SQL Server.

El ambiente local debe tener un solo contenedor SQL Server: `sqlserver` de PortalCorporativo. Financiero usa una base lógica independiente `FinancieroDb` dentro de ese servidor mediante `host.docker.internal:${PORTAL_SQLSERVER_PORT:-1433}`. Portal se configura mediante `PORTAL_GATEWAY_BASE_URL`; Seq también es externo. Las flags Portal permanecen `false` mientras solo existan adaptadores de desarrollo.

## Seguridad

`JWT_SECRET` es obligatorio y nunca se versiona. No existe login local: tokens provienen de Portal/IdP. Los futuros endpoints contables deberán exigir permisos `financial.*`; health permanece anónimo.

## Alcance P0

Los adaptadores Portal son no-op seguros; Security falla cerrado. No habilitar flags en producción hasta implementar clientes HTTP/Outbox reales y pruebas de contrato. No existen entidades ni migraciones contables todavía.
