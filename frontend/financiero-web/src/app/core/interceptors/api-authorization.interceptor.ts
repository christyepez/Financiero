import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { PortalAuthAdapter } from '../adapters/portal-auth.adapter';

export const apiAuthorizationInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(PortalAuthAdapter);
  const token = auth.getAccessToken();
  const devPermissions = auth.getDevelopmentPermissions();
  let headers = request.headers;

  if (token) {
    headers = headers.set('Authorization', `Bearer ${token}`);
  }

  if (devPermissions) {
    headers = headers.set('X-Dev-Permissions', devPermissions);
  }

  return next(request.clone({ headers }));
};
