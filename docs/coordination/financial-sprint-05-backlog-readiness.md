# Financial Sprint 5 Backlog Readiness

Default recommendation: Option A unless external gates approve a more sensitive option.

## Option A - Purchases and voided documents foundation

Recommended when there is no approved certificate custody, SRI Test evidence or legal RIDE approval.

- Implement purchases foundation.
- Implement voided documents foundation.
- Reduce ATS critical gaps.
- Keep everything non-productive and synthetic-data safe.

## Option B - XAdES + SRI Test controlled send

Only if a non-production certificate, secure custody and approval exist.

- Implement controlled real signing.
- Execute SRI Test with synthetic data.
- Capture sanitized evidence.
- Keep production blocked.

## Option C - Portal Content/File real upload

Only if Portal exposes a stable contract.

- Enable `dryRun=false` in a controlled environment.
- Keep `sendPayloads=false` by default.
- Validate metadata/hash first.
- Send real payload only with explicit approval.

## Option D - RIDE legal final

Only with legal/tax review.

- Final layout.
- Official QR validation.
- Final PDF.
- Approved evidence.

## Option E - ATS official XML foundation

Only with reviewed official schema/catalogs.

- Generate non-production ATS XML foundation.
- Validate structure/XSD.
- Do not submit officially.

## Planning rule

Choose Option A by default. Choose Options B/C/D/E only when their external review gates are approved and documented.
