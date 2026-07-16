import { Component, Input } from '@angular/core';

@Component({
  selector: 'fin-error-message',
  standalone: true,
  template: `@if (message) { <div class="error">{{ message }}</div> }`
})
export class ErrorMessageComponent {
  @Input() message: string | null = null;
}
