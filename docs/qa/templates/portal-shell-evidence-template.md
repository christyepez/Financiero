# Portal Shell Evidence Template

## What to capture

- Owner name/role.
- Date/time.
- Portal Shell base URL, sanitized.
- Health/base route result.
- Confirmation Shell can navigate to Financiero route.

## What to redact

- Tokens, cookies, session identifiers, personal data and private URLs.

## Do not include

- Screenshots with logged-in users, tokens, secrets, real taxpayer data or production URLs.

## Suggested commands

```powershell
Invoke-WebRequest -UseBasicParsing http://localhost:4201/health
Invoke-WebRequest -UseBasicParsing http://localhost:4201
```

Use the real Portal-provided route if different.

## Expected result

- Portal Shell responds HTTP 2xx.
- Financiero route is reachable through Portal navigation.

## Acceptance criteria

- Live Shell evidence is sanitized.
- Shell is externally owned by Portal.
- Financiero does not create a duplicate Shell.

## Invalid evidence

- Standalone Financiero UI treated as Portal Shell.
- Token-bearing URLs.
- Shell clone inside Financiero.

Control tokens: BLOCKED_DEPENDENCY; Portal Gateway; shared SQL; not production-ready; No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.
