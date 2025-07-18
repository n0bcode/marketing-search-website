import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SecretTokenResponseDTO } from '../../models/dtos/secret-token-dto/secret-token-response-dto';

@Component({
  selector: 'app-secret-token-panel',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './secret-token-panel.component.html',
})
export class SecretTokenPanelComponent {
  @Input() isShowViewSettingToken: boolean = false;
  @Input() listServices: string[] = [];
  @Input() listSecretDTOsMap: Record<string, SecretTokenResponseDTO[]> = {};
  @Input() listSelectSecretToken: Record<string, string> = {};
  @Input() secretTokenDTO: { name: string; token: string } = { name: '', token: '' };
  @Output() toggleSettings = new EventEmitter<void>();
  @Output() createToken = new EventEmitter<string | null>();
}
