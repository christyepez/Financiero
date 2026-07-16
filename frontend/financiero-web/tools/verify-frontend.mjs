import { readFileSync, readdirSync, statSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { join } from 'node:path';

const root = fileURLToPath(new URL('..', import.meta.url));
const required = [
  'src/app/core/adapters/portal-auth.adapter.ts',
  'src/app/core/adapters/portal-menu.adapter.ts',
  'src/app/core/interceptors/api-authorization.interceptor.ts',
  'src/app/core/interceptors/error-sanitization.interceptor.ts',
  'src/app/features/sri-readiness/sri-readiness.component.ts',
  'src/app/features/ats-readiness/ats-readiness.component.ts',
  'src/app/features/external-approvals/external-approvals.component.ts',
  'src/environments/environment.ts'
];

for (const file of required) {
  statSync(join(root, file));
}

const forbidden = [
  'localStorage',
  'sessionStorage',
  'financial-login',
  'BEGIN PRIVATE KEY',
  '.p12',
  '.pfx',
  '<factura'
];

function walk(directory) {
  return readdirSync(directory).flatMap(name => {
    const full = join(directory, name);
    const stat = statSync(full);
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

console.log('Frontend foundation checks passed.');
