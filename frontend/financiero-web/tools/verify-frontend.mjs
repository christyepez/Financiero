import { readFileSync, readdirSync, statSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { basename, join } from 'node:path';

const root = fileURLToPath(new URL('..', import.meta.url));
const repoRoot = fileURLToPath(new URL('../../..', import.meta.url));
const required = [
  'src/app/core/adapters/portal-auth.adapter.ts',
  'src/app/core/adapters/portal-menu.adapter.ts',
  'src/app/core/interceptors/api-authorization.interceptor.ts',
  'src/app/core/interceptors/error-sanitization.interceptor.ts',
  'src/app/core/services/sanitization.service.ts',
  'src/app/core/portal-shell/portal-shell.models.ts',
  'src/app/core/portal-shell/portal-context.adapter.ts',
  'src/app/core/adapters/portal-configuration.adapter.ts',
  'src/app/core/adapters/portal-feature-flag.adapter.ts',
  'src/app/core/adapters/portal-telemetry.adapter.ts',
  'src/app/core/services/command-guard.service.ts',
  'src/app/shared/components/loading-state.component.ts',
  'src/app/shared/components/period-selector.component.ts',
  'src/app/shared/components/shell-mode-banner.component.ts',
  'src/app/shared/components/command-disabled-banner.component.ts',
  'src/app/shared/components/foundation-command-disclaimer.component.ts',
  'src/app/features/purchases/purchase-create-form.component.ts',
  'src/app/features/voided-documents/voided-document-create-form.component.ts',
  'src/app/features/sri-readiness/sri-readiness.component.ts',
  'src/app/features/ats-readiness/ats-readiness.component.ts',
  'src/app/features/external-approvals/external-approvals.component.ts',
  'src/environments/environment.ts',
  'src/environments/environment.development.ts',
  'src/environments/environment.example.ts'
];

for (const file of required) {
  statSync(join(root, file));
}

const forbidden = [
  'localStorage',
  'sessionStorage',
  'document.cookie',
  'console.log',
  'financial-login',
  'BEGIN PRIVATE KEY',
  'BEGIN CERTIFICATE',
  '.p12',
  '.pfx',
  '.pem',
  '.key',
  'sri.gob.ec',
  '<factura'
];

function walk(directory) {
  return readdirSync(directory).flatMap(name => {
    const full = join(directory, name);
    const stat = statSync(full);
    if (stat.isDirectory() && ['node_modules', 'dist', '.angular', 'coverage'].includes(basename(full))) {
      return [];
    }
    return stat.isDirectory() ? walk(full) : [full];
  });
}

for (const file of walk(join(root, 'src'))) {
  const text = readFileSync(file, 'utf8');
  for (const pattern of forbidden) {
    if (text.includes(pattern)) {
      throw new Error(`Forbidden frontend pattern "${pattern}" found in ${file}`);
    }
  }
}

for (const file of walk(root)) {
  if (file.endsWith('.env') || file.endsWith('.env.local')) {
    throw new Error(`Environment secret file must not be committed: ${file}`);
  }
  if (/\.(p12|pfx|pem|key|cer|crt)$/i.test(file)) {
    throw new Error(`Certificate or key file must not be committed: ${file}`);
  }
}

const apiService = readFileSync(join(root, 'src/app/core/services/api.service.ts'), 'utf8');
if (!apiService.includes('ApiEnvelope') || !apiService.includes('unwrap')) {
  throw new Error('ApiService must unwrap backend ApiResponse envelopes.');
}

const authAdapter = readFileSync(join(root, 'src/app/core/adapters/portal-auth.adapter.ts'), 'utf8');
if (!authAdapter.includes("environment.production || context.environment.shellMode === 'portal-integrated'")) {
  throw new Error('Development permissions must be disabled in production.');
}

const portalModels = readFileSync(join(root, 'src/app/core/portal-shell/portal-shell.models.ts'), 'utf8');
for (const token of ['PortalShellContext', 'PortalFeatureFlags', 'PORTAL_SHELL_CONTEXT', '__PORTAL_SHELL_CONTEXT__']) {
  if (!portalModels.includes(token)) throw new Error(`Portal shell contract is missing ${token}.`);
}

const portalDefaults = readFileSync(join(root, 'src/app/core/portal-shell/portal-shell.defaults.ts'), 'utf8');
for (const safeFlag of ['allowXmlPreviewUi: false', 'allowMutations: false', 'allowDevHeaders: false']) {
  if (!portalDefaults.includes(safeFlag)) throw new Error(`Safe feature flag default missing: ${safeFlag}.`);
}

for (const safeFlag of ['allowPurchaseCommands: false', 'allowVoidedDocumentCommands: false', 'allowAtsOfficialActions: false', 'allowSriSubmission: false']) {
  if (!portalDefaults.includes(safeFlag)) throw new Error(`Command safety flag default missing: ${safeFlag}.`);
}

const prodEnvironment = readFileSync(join(root, 'src/environments/environment.ts'), 'utf8');
for (const token of ['production: true', 'enableDevHeaders: false', 'allowXmlPreview: false']) {
  if (!prodEnvironment.includes(token)) throw new Error(`Production environment must keep ${token}.`);
}
if (prodEnvironment.includes('financial.electronicdocuments.manage')) {
  throw new Error('Production environment must not include development permissions.');
}

const commandGuard = readFileSync(join(root, 'src/app/core/services/command-guard.service.ts'), 'utf8');
for (const token of ['allowMutations', 'allowPurchaseCommands', 'allowVoidedDocumentCommands', 'financial.electronicdocuments.manage']) {
  if (!commandGuard.includes(token)) throw new Error(`Command guard missing ${token}.`);
}

const externalApproval = readFileSync(join(root, 'src/app/core/services/external-approval-api.service.ts'), 'utf8');
if (!externalApproval.includes('/api/financial/external-approvals/readiness')) {
  throw new Error('External approval readiness must use the real backend route.');
}

for (const doc of [
  'docs/releases/financial-sprint-06-closure.md',
  'docs/releases/financial-sprint-06-release-notes.md',
  'docs/qa/financial-sprint-06-qa-evidence.md',
  'docs/architecture/financial-sprint-06-architecture-snapshot.md',
  'docs/coordination/financial-sprint-06-p5-closure-ux-portal-readiness.md',
  'docs/frontend/portal-shell-readiness-matrix.md',
  'docs/frontend/portal-shell-contract.md'
]) {
  statSync(join(repoRoot, doc));
}

const foundationDisclaimer = readFileSync(join(root, 'src/app/shared/components/foundation-disclaimer.component.ts'), 'utf8');
if (!foundationDisclaimer.includes('Foundation / No productivo')) {
  throw new Error('Foundation disclaimer must remain explicit.');
}

console.log('Frontend foundation checks passed.');
