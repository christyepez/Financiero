import { Component, Input } from '@angular/core';

@Component({
  selector: 'fin-portal-context-error',
  standalone: true,
  template: `
    @if (warnings.length) {
      <section class="warning">
        <span class="badge warn">Portal contract warning</span>
        <ul>
          @for (warning of warnings; track warning) {
            <li>{{ warning }}</li>
          }
        </ul>
      </section>
    }
  `
})
export class PortalContextErrorComponent {
  @Input() warnings: string[] = [];
}
