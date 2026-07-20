# Portal Gateway Evidence Template

## What to capture

- Owner name/role.
- Date/time.
- Portal Gateway base URL, sanitized.
- Confirmed health path.
- HTTP status for health route.
- Correlation id behavior if visible without secrets.

## What to redact

- Tokens.
- Cookies.
- Private URLs that are not approved for repo documentation.
- Headers containing identity or secrets.

## Do not include

- Access tokens, id tokens, refresh tokens, passwords, real user data, screenshots with sessions or private URLs.

## Suggested commands

```powershell
Invoke-WebRequest -UseBasicParsing http://localhost:8082/health
Invoke-WebRequest -UseBasicParsing http://localhost:8082/health/live
Invoke-WebRequest -UseBasicParsing http://localhost:8082/health/ready
Invoke-WebRequest -UseBasicParsing http://localhost:8082/api/health
```

## Expected result

- One owner-confirmed health route returns HTTP 2xx.
- Route is documented for preflight `-PortalGatewayHealthPath`.

## Acceptance criteria

- HTTP 2xx at confirmed route.
- Sanitized URL/path/status only.
- No tokens or private session details.

## Invalid evidence

- HTTP 404 without route decision.
- Screenshots with tokens.
- Gateway implemented inside Financiero.

Control tokens: BLOCKED_DEPENDENCY; Portal Gateway; shared SQL; not production-ready; No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.
