import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class SanitizationService {
  maskIdentifier(value: string | null | undefined): string {
    if (!value) return '';
    const compact = value.replace(/\D/g, '');
    if (compact.length <= 4) return '*'.repeat(compact.length);
    return `${'*'.repeat(Math.max(0, compact.length - 4))}${compact.slice(-4)}`;
  }

  maskAccessKey(value: string | null | undefined): string {
    if (!value) return '';
    const compact = value.replace(/\s/g, '');
    if (compact.length <= 8) return '[masked]';
    return `${'*'.repeat(Math.max(0, compact.length - 8))}${compact.slice(-8)}`;
  }

  safeText(value: unknown): string {
    const text = String(value ?? '');
    return text
      .replace(/<[^>]+>/g, '[xml-redacted]')
      .replace(/(token|secret|password|certificate|claveAcceso|accessKey)[^,;\s]*/gi, '[redacted]');
  }
}
