import { Component, Input } from '@angular/core';

@Component({
  selector: 'fin-validation-issues-list',
  standalone: true,
  template: `
    @if (issues.length) {
      <ul class="list">
        @for (issue of issues; track $index) {
          <li>{{ issue }}</li>
        }
      </ul>
    }
  `
})
export class ValidationIssuesListComponent {
  @Input() issues: string[] = [];
}
