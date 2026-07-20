# SQL Common Evidence Template

## What to capture

- Owner name/role.
- Date/time.
- Shared SQL host and port, sanitized.
- TCP connectivity result.
- Logical database `FinancieroDb` availability.
- Confirmation that Financiero does not define its own SQL Server container.

## What to redact

- Passwords.
- Full connection strings.
- Usernames if sensitive.
- Private hostnames if not approved for repo documentation.

## Do not include

- Tokens, secrets, certificates, XML reales, personal data, real SRI responses or screenshots with credentials.

## Suggested commands

```powershell
Test-NetConnection -ComputerName host.docker.internal -Port 21433
docker ps --format "table {{.Names}}\t{{.Ports}}"
```

SQL connectivity proof must be sanitized and must not include credentials.

## Expected result

- TCP PASS for `host.docker.internal:21433`.
- `FinancieroDb` confirmed.
- No Financiero SQL Server container.

## Acceptance criteria

- Evidence is text-only and sanitized.
- Owner and timestamp included.
- PASS is reproducible.

## Invalid evidence

- Full connection strings.
- Passwords/tokens.
- New SQL Server inside Financiero.
- Shared DB across domains.

Control tokens: BLOCKED_DEPENDENCY; Portal Gateway; shared SQL; not production-ready; No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.
