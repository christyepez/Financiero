# Financial QA Environment Template

## Purpose

Document the variables expected for local/QA validation without committing secrets. Store real values only in local untracked files or a secret store.

## Template

```powershell
FINANCIAL_API_BASE_URL=http://localhost:8083
FINANCIAL_WEB_BASE_URL=http://localhost:4200
PORTAL_GATEWAY_BASE_URL=http://localhost:8082
PORTAL_SHELL_BASE_URL=http://localhost:4201
SHARED_SQL_HOST=host.docker.internal
SHARED_SQL_PORT=21433
SHARED_SQL_DATABASE=FinancieroDb
SEQ_URL=http://localhost:5342
REDIS_URL=
```

## Where to store local values

- Use `.env`, `.env.local`, PowerShell profile variables or OS environment variables.
- Do not commit `.env` or `.env.local`.
- Do not paste passwords, tokens, certificates, real connection strings or taxpayer data into docs.

## Variable validation

| Variable | Validation |
|---|---|
| `FINANCIAL_API_BASE_URL` | `GET /health/live` when API is running. |
| `FINANCIAL_WEB_BASE_URL` | Browser reaches Angular app. |
| `PORTAL_GATEWAY_BASE_URL` | `GET /health` or configured Portal health path. |
| `PORTAL_SHELL_BASE_URL` | Browser reaches Portal shell. |
| `SHARED_SQL_HOST` | DNS resolves from host. |
| `SHARED_SQL_PORT` | TCP check succeeds. |
| `SHARED_SQL_DATABASE` | `FinancieroDb` exists as logical DB only. |
| `SEQ_URL` | Optional log inspection endpoint. |
| `REDIS_URL` | Optional Portal-owned dependency. |

## Do not upload

- Tokens.
- Passwords.
- Full connection strings.
- Certificates or keys.
- XML or SRI responses.
- Real evidence files.
