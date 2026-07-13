# Sprint 2 QA Evidence - SRI Readiness

## Matriz de validaciones

| Paquete | Tests reportados | Estado |
|---|---:|---|
| P1 SRI foundation | 75/75 | OK |
| P2 XAdES/SRI/storage | 81/81 | OK |
| P3 XAdES/SOAP/ContentFile/RIDE | 88/88 | OK |
| P4 secure integration readiness | 99/99 | OK |
| P5 secret store/SRI observability | 106/106 | OK |
| P6 readiness closure docs | 106/106 | OK con `DOTNET_ROLL_FORWARD=Major` |

## Validado

- `dotnet restore`.
- `dotnet build`.
- `dotnet test` sin roll-forward: bloqueado por runtime local sin .NET 8.
- `DOTNET_ROLL_FORWARD=Major dotnet test`: 106/106 OK.
- `git diff --check`.
- `docker compose config`.
- SQL Server común: no hay contenedor SQL propio de Financiero.
- Sin secretos/certificados/XML reales detectados.

## No validado como OK

- `docker compose up -d --build`.
- `/health`, `/health/live`, `/health/ready`, `/health/sri`.
- Smoke SRI mock en contenedor.

## Bloqueo

Desde P3 a P6 Docker build quedó bloqueado por timeout externo resolviendo `mcr.microsoft.com/dotnet/aspnet:8.0`. No se declara health/smoke OK.

## Evidencia mínima para cerrar Sprint 2

Build y tests .NET OK, compose config OK, SQL común validado, seguridad sin material sensible y documentación de riesgo Docker.

## Evidencia requerida antes de Sprint 3 funcional tributario

Reintentar Docker cuando MCR esté disponible, ejecutar health/smoke, validar SRI readiness y conservar evidencia sanitizada.
