import { Component, Input } from '@angular/core';
import { GeminiResponse } from '../../interfaces/geminiAiService/gemini-response';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-modal-analysis-media-social',
  imports: [CommonModule],
  templateUrl: './modal-analysis-media-social.component.html',
  styleUrl: './modal-analysis-media-social.component.css',
})
export class ModalAnalysisMediaSocialComponent {
  /**
   * Nhận dữ liệu truyền vào component (ví dụ: thông tin link, kết quả phân tích, v.v.)
   */
  @Input() data!: GeminiResponse | null;
  @Input() isShowModal: boolean = false;
  @Input() isLoading: boolean = false;
}
