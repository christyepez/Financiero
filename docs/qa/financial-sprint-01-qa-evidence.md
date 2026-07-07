# Financial Sprint 1 QA Evidence

## Build

Resultado reportado en P5:

```text
dotnet build Financiero.sln
Compilación correcta. 0 Advertencia(s), 0 Errores.
```

## Tests

Resultado reportado en P5:

```text
dotnet test Financiero.sln --no-build
Correctas: 61/61
```

Distribución reportada:

- Domain: 19.
- Application: 31.
- API: 11.

## Docker compose

```text
docker compose config --quiet
OK
```

## Health y readiness

```text
GET http://localhost:8083/health/ready
200
```

## Docker smoke

```text
scripts/smoke/financial-smoke.ps1 -BaseUrl http://localhost:8083
Financial smoke OK.
```

## Security smoke

Validado en P5:

- Permisos positivos por endpoint.
- Denegación sin permisos suficientes.
- Wildcard `financial.*` en Development con `X-Dev-Permissions`.

## Integrated accounting flow

Validado en P5:

- Crear cuentas.
- Crear y abrir año/período fiscal.
- Crear asiento.
- Agregar líneas.
- Postear asiento.
- Reversar asiento.
- Validar Audit/Outbox y correlationId.

## SQL concurrency

La numeración usa transacción serializable con `UPDLOCK,HOLDLOCK` y restricción única para evitar duplicados.

## Migration runner

Runner básico aplica scripts versionados y registra historial en `financial.schema_versions`.

## Criterios QA aprobados

- Build OK.
- Tests OK.
- Docker Compose válido.
- Health/readiness OK.
- Smoke Docker OK.
- Seguridad runtime cubierta por tests.
- No se detectó SQL Server duplicado.

## Riesgos QA restantes

- Pruebas de carga/concurrencia extendidas pendientes.
- Validación productiva de Portal HTTP adapters pendiente.
- Validación de migraciones con herramienta formal pendiente.
