import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

const sensitivePatterns = [
  /claveAcceso/gi,
  /accessKey/gi,
  /authorizationXml/gi,
  /signedXml/gi,
  /unsignedXml/gi,
  /certificate/gi,
  /password/gi,
  /token/gi,
  /secret/gi,
  /<[^>]+>/g
];

export const errorSanitizationInterceptor: HttpInterceptorFn = (request, next) =>
  next(request).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse) {
        const raw = typeof error.error === 'string' ? error.error : error.message;
        const sanitized = sensitivePatterns.reduce((value, pattern) => value.replace(pattern, '[redacted]'), raw);
        return throwError(() => new Error(sanitized || 'Financial API request failed.'));
      }

      return throwError(() => new Error('Financial API request failed.'));
    })
  );
