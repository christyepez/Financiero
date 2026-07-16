import { Component, Input } from '@angular/core';

@Component({
  selector: 'fin-error-message',
  standalone: true,
  template: `
    @if (message) {
      <div class="error">
        <strong>No se pudo completar la consulta foundation.</strong>
        <div>{{ message }}</div>
        <small>El detalle fue sanitizado para no exponer XML, access keys, identificaciones completas, tokens ni secretos.</small>
      </div>
    }
  `
})
export class ErrorMessageComponent {
  @Input() message: string | null = null;
}
