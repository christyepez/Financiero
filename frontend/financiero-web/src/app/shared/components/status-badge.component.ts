import { Component, Input } from '@angular/core';

@Component({
  selector: 'fin-status-badge',
  standalone: true,
  template: `<span class="badge" [class]="className">{{ value || 'Unknown' }}</span>`
})
export class StatusBadgeComponent {
  @Input() value = '';

  get className(): string {
    const normalized = this.value.toLowerCase();
    if (normalized.includes('ready') || normalized.includes('ok') || normalized.includes('integrated') || normalized.includes('approved')) return 'badge ok';
    if (normalized.includes('error') || normalized.includes('blocked') || normalized.includes('rejected')) return 'badge bad';
    if (normalized.includes('warn') || normalized.includes('pending') || normalized.includes('foundation')) return 'badge warn';
    return 'badge info';
  }
}
