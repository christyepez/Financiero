import { Component } from '@angular/core';

@Component({
  selector: 'fin-foundation-command-disclaimer',
  standalone: true,
  template: `
    <div class="warning">
      Foundation only · No oficial SRI · No envía datos al SRI · No genera ATS oficial · Requiere revisión tributaria.
    </div>
  `
})
export class FoundationCommandDisclaimerComponent {}
