import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { PortalMenuAdapter } from './core/adapters/portal-menu.adapter';
import { PortalNotificationAdapter } from './core/adapters/portal-notification.adapter';
import { StatusBadgeComponent } from './shared/components/status-badge.component';
import { environment } from '../environments/environment';

@Component({
  selector: 'fin-root',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, RouterOutlet, StatusBadgeComponent],
  template: `
    <div class="shell">
      <aside class="sidebar">
        <div class="brand">Financiero</div>
        <div class="caption">Angular Shell foundation consumiendo PortalCorporativo para seguridad, menú y notificaciones.</div>
        <nav class="nav" aria-label="Financial navigation">
          @for (item of menu.items(); track item.path) {
            <a [routerLink]="item.path" routerLinkActive="active">{{ item.label }}</a>
          }
        </nav>
      </aside>

      <main class="main">
        <header class="topbar">
          <div>
            <h1>{{ menu.currentTitle() }}</h1>
            <div class="muted">Modo read-only foundation. Sin login propio, sin XML completo, sin envío SRI producción.</div>
          </div>
          <fin-status-badge [value]="notifications.foundationStatus()" />
        </header>

        @if (showWarnings) {
          <div class="warning">
            Esta UI es una base de integración. Las funciones SRI/ATS/RIDE son readiness técnico y requieren aprobación externa antes de producción.
          </div>
        }

        <router-outlet />
      </main>
    </div>
  `
})
export class AppComponent {
  protected readonly menu = inject(PortalMenuAdapter);
  protected readonly notifications = inject(PortalNotificationAdapter);
  protected readonly showWarnings = environment.showFoundationWarnings;
}
