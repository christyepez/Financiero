import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { PortalTelemetryAdapter } from '../adapters/portal-telemetry.adapter';

export const correlationIdInterceptor: HttpInterceptorFn = (request, next) => {
  const value = inject(PortalTelemetryAdapter).correlationId() || globalThis.crypto?.randomUUID?.() || `fin-${Date.now()}-${Math.random().toString(16).slice(2)}`;
  return next(request.clone({ headers: request.headers.set('X-Correlation-ID', value) }));
};
