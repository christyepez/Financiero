import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { AppComponent } from './app/app.component';
import { routes } from './app/app.routes';
import { apiAuthorizationInterceptor } from './app/core/interceptors/api-authorization.interceptor';
import { correlationIdInterceptor } from './app/core/interceptors/correlation-id.interceptor';
import { errorSanitizationInterceptor } from './app/core/interceptors/error-sanitization.interceptor';

bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([
      correlationIdInterceptor,
      apiAuthorizationInterceptor,
      errorSanitizationInterceptor
    ]))
  ]
}).catch((error: unknown) => console.error('Angular bootstrap failed.', error));
