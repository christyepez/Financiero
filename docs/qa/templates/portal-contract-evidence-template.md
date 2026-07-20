# Portal Contract Evidence Template

## What to capture

- Owner name/role.
- Date/time.
- Sanitized `PortalShellContext`.
- `contractVersion`.
- `source=portal`.
- Tenant/user identifiers only if synthetic or redacted.
- Permissions.
- Menu routes.
- Feature flags.
- Correlation id.

## What to redact

- Delegated tokens.
- Real user identifiers.
- Personal data.
- Cookies and session headers.

## Do not include

- Tokens, passwords, real personal data, certificates, XML reales, SRI responses or private URLs.

## Suggested capture

Provide sanitized JSON-like text:

```json
{
  "contractVersion": "1.0",
  "source": "portal",
  "permissions": ["financial.electronicdocuments.read"],
  "menu": ["/dashboard"],
  "featureFlags": {
    "allowProductiveActivation": false,
    "allowSriSubmission": false
  },
  "correlationId": "synthetic-correlation-id"
}
```

## Expected result

- Context is accepted by Financiero PortalShellContext validation.
- Productive actions remain disabled.

## Acceptance criteria

- Sanitized live evidence.
- Menu routes match Financiero allow-list.
- Permissions match expected financial permissions.
- No token storage.

## Invalid evidence

- Context generated only by Financiero standalone mode.
- Tokens or real personal data.
- Productive feature flags enabled.

Control tokens: BLOCKED_DEPENDENCY; Portal Gateway; shared SQL; not production-ready; No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.
