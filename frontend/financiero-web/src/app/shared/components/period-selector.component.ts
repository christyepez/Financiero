import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'fin-period-selector',
  standalone: true,
  imports: [FormsModule],
  template: `
    <label class="period-selector">
      <span>Período</span>
      <input type="month" [ngModel]="period" (ngModelChange)="periodChange.emit($event)" />
    </label>
  `
})
export class PeriodSelectorComponent {
  @Input() period = '';
  @Output() periodChange = new EventEmitter<string>();
}
