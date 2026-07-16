import { Component, Input } from '@angular/core';

@Component({
  selector: 'fin-permission-hint',
  standalone: true,
  template: `<span class="muted"> · permisos: {{ label }}</span>`
})
export class PermissionHintComponent {
  @Input() permissions: string[] = [];

  get label(): string {
    if (!this.permissions.length) return 'delegados/no locales';
    return `${this.permissions.length} permisos foundation`;
  }
}
