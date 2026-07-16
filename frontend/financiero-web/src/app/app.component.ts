import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { PortalMenuAdapter } from './core/adapters/portal-menu.adapter';
import { PortalNotificationAdapter } from './core/adapters/portal-notification.adapter';
import { ShellModeBannerComponent } from './shared/components/shell-mode-banner.component';
import { StatusBadgeComponent } from './shared/components/status-badge.component';
import { environment } from '../environments/environment';
import { PortalContextAdapter } from './core/portal-shell/portal-context.adapter';
import { PortalContextErrorComponent } from './shared/components/portal-context-error.component';
import { PortalContextRequiredComponent } from './shared/components/portal-context-required.component';

@Component({
  selector: 'fin-root',
  standalone: true,
  imports: [PortalContextErrorComponent, PortalContextRequiredComponent, RouterLink, RouterLinkActive, RouterOutlet, ShellModeBannerComponent, StatusBadgeComponent],
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
          <fin-shell-mode-banner />
          <fin-portal-context-error [warnings]="portal.contextWarnings()" />
        }

        @if (portal.missingRequiredPortalContext() || portal.hasUnsupportedContract()) {
          <fin-portal-context-required />
        } @else {
          <router-outlet />
        }
      </main>
    </div>
  `
})
export class AppComponent {
  protected readonly menu = inject(PortalMenuAdapter);
  protected readonly notifications = inject(PortalNotificationAdapter);
  protected readonly portal = inject(PortalContextAdapter);
  protected readonly showWarnings = environment.showFoundationWarnings;
}
