import { HttpInterceptorFn } from '@angular/common/http';

export const correlationIdInterceptor: HttpInterceptorFn = (request, next) => {
  const value = globalThis.crypto?.randomUUID?.() ?? `fin-${Date.now()}-${Math.random().toString(16).slice(2)}`;
  return next(request.clone({ headers: request.headers.set('X-Correlation-ID', value) }));
};
