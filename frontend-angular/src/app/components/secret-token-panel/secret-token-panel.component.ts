import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-secret-token-panel',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './secret-token-panel.component.html',
})
export class SecretTokenPanelComponent {
  @Input() isShowViewSettingToken!: boolean;
  @Input() listServices!: string[];
  @Input() listSecretDTOsMap!: { [key: string]: any[] };
  @Input() listSelectSecretToken!: { [key: string]: string };
  @Input() secretTokenDTO!: { name: string; token: string };
  @Output() toggleSettings = new EventEmitter<void>();
  @Output() createToken = new EventEmitter<string>();
}