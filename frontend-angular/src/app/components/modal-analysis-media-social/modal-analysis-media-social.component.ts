import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-modal-analysis-media-social',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './modal-analysis-media-social.component.html',
})
export class ModalAnalysisMediaSocialComponent {
  @Input() isShowModal: boolean = false;
  @Input() isLoading: boolean = false;
  @Input() data: any = null;
  @Output() modalToggle = new EventEmitter<void>();

  // Biến debug, có thể bật/tắt trong môi trường phát triển
  isDebug: boolean = false;

  openModal() {
    this.isShowModal = true;
    this.modalToggle.emit();
  }

  closeModal() {
    this.isShowModal = false;
    this.modalToggle.emit();
  }
}
