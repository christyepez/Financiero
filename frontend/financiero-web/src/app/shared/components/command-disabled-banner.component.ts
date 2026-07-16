import { Component, Input } from '@angular/core';

@Component({
  selector: 'fin-command-disabled-banner',
  standalone: true,
  template: `
    <div class="warning">
      <strong>Modo read-only activo.</strong> {{ reason }}
      No se envía información al SRI, no se genera ATS oficial y no se ejecutan mutaciones productivas.
    </div>
  `
})
export class CommandDisabledBannerComponent {
  @Input() reason = 'Los comandos foundation están deshabilitados por configuración segura.';
}
