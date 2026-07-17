import { readFileSync, statSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { join } from 'node:path';

const root = fileURLToPath(new URL('..', import.meta.url));
const repoRoot = fileURLToPath(new URL('../../..', import.meta.url));

const files = {
  models: 'src/app/core/portal-shell/portal-shell.models.ts',
  validation: 'src/app/core/portal-shell/portal-shell-validation.ts',
  defaults: 'src/app/core/portal-shell/portal-shell.defaults.ts',
  context: 'src/app/core/portal-shell/portal-context.adapter.ts',
  auth: 'src/app/core/adapters/portal-auth.adapter.ts',
  menu: 'src/app/core/adapters/portal-menu.adapter.ts',
  guard: 'src/app/core/services/command-guard.service.ts',
  api: 'src/app/core/services/portal-integration-readiness-api.service.ts',
  dashboard: 'src/app/features/dashboard/dashboard.component.ts',
  app: 'src/app/app.component.ts'
};

for (const file of Object.values(files)) statSync(join(root, file));

const read = file => readFileSync(join(root, file), 'utf8');
const models = read(files.models);
const validation = read(files.validation);
const defaults = read(files.defaults);
const context = read(files.context);
const auth = read(files.auth);
const menu = read(files.menu);
const guard = read(files.guard);
const api = read(files.api);
const dashboard = read(files.dashboard);
const app = read(files.app);
const externalApproval = readFileSync(join(root, 'src/app/features/external-approvals/external-approvals.component.ts'), 'utf8');

for (const token of ['SUPPORTED_PORTAL_SHELL_CONTRACT_VERSION', 'PortalShellSource', "'standalone' | 'portal'", 'issuedAt', 'expiresAt', 'permissions', 'menu', 'featureFlags', 'correlationId']) {
  if (!models.includes(token)) throw new Error(`PortalShellContext contract missing ${token}.`);
}
for (const token of ['sanitizeContractVersion', 'sanitizeMenuItems', 'sanitizeDelegatedToken', 'Portal integrated context missing required user']) {
  if (!validation.includes(token)) throw new Error(`PortalShellContext validation missing ${token}.`);
}
for (const flag of ['allowProductizationReadiness: true', 'allowPortalContentFileEvidenceReferences: false', 'allowPortalNotificationIntents: false', 'allowExternalApprovalCommands: false', 'allowPurchaseCommands: false', 'allowVoidedDocumentCommands: false', 'allowProductiveActivation: false', 'allowOfficialTaxFlows: false', 'allowSriSubmission: false', 'allowAtsOfficialActions: false', 'allowNotificationSend: false', 'allowEvidenceUpload: false']) {
  if (!defaults.includes(flag)) throw new Error(`Safe feature flag default missing ${flag}.`);
}
for (const token of ["context.environment.shellMode !== 'portal-integrated'", 'context.expiresAt', 'environment.production || context.environment.shellMode ===']) {
  if (!auth.includes(token)) throw new Error(`Delegated auth safety missing ${token}.`);
}
for (const token of ['allowedFinancialRoutes', '/dashboard', '/purchases', '/voided-documents']) {
  if (!defaults.includes(token)) throw new Error(`Menu allow-list definition missing ${token}.`);
}
for (const token of ['hasPermission', "!item.route.startsWith('//')"]) {
  if (!menu.includes(token)) throw new Error(`Menu allow-list validation missing ${token}.`);
}
for (const token of ['canActivateProduction', 'allowProductiveActivation', 'allowOfficialTaxFlows']) {
  if (!guard.includes(token)) throw new Error(`Command guard production block missing ${token}.`);
}
for (const token of ['/api/financial/portal-integration/readiness', 'Portal E2E readiness', 'development standalone only', 'production requires Portal context', 'SQL común requerido', 'BLOCKED DEPENDENCY']) {
  const combined = `${api}\n${dashboard}`;
  if (!combined.includes(token)) throw new Error(`Portal E2E UI/API missing ${token}.`);
}
for (const token of ['ApprovedFoundation no habilita producción', 'Evidence reference is Portal-owned metadata only', 'Notification intent is prepared only; no send', 'Portal Notification owner', 'No file stored in Financiero']) {
  if (!externalApproval.includes(token)) throw new Error(`External approval Portal boundary missing ${token}.`);
}
for (const token of ['missingRequiredPortalContext()', 'hasUnsupportedContract()', 'fin-portal-context-required']) {
  if (!app.includes(token)) throw new Error(`Production standalone block missing ${token}.`);
}
for (const doc of [
  'docs/integration/portal-e2e-validation-checklist.md',
  'docs/runbooks/portal-financiero-local-e2e-runbook.md',
  'docs/qa/financial-sprint-08-p1-e2e-evidence-template.md',
  'docs/qa/financial-sprint-08-p2-e2e-execution-evidence.md',
  'docs/runbooks/shared-sql-runtime-validation.md',
  'docs/runbooks/financial-qa-env-template.md',
  'docs/runbooks/financial-health-troubleshooting.md',
  'docs/qa/financial-sprint-08-p3-qa-infra-stabilization-evidence.md',
  'docs/qa/financial-sprint-08-p4-external-approval-ux-evidence.md',
  'tools/validate-portal-financiero-e2e.ps1'
]) statSync(join(repoRoot, doc));

const forbiddenQueryTokens = ['access_token=', 'id_token=', 'refresh_token=', 'token='];
for (const file of [models, validation, context, auth, api, dashboard]) {
  for (const token of forbiddenQueryTokens) {
    if (file.includes(token)) throw new Error(`Querystring token pattern must not appear: ${token}.`);
  }
}

console.log('Portal E2E contract checks passed.');
