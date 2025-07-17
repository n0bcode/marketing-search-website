import {
  Component,
  Input,
  Output,
  EventEmitter,
  ViewChild,
  ElementRef,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { GoogleSearchRequest } from '../../interfaces/googleSearchService/google-search-request';
import { SearchRequest } from '../../interfaces/search-request';

@Component({
  selector: 'app-search-form',
  standalone: true,
  imports: [CommonModule, FormsModule, MatProgressSpinnerModule],
  templateUrl: './search-form.component.html',
  styleUrl: './search-form.component.css',
})
export class SearchFormComponent {
  @Input() searchParameters: GoogleSearchRequest | undefined;
  @Output() searchParametersChange = new EventEmitter<GoogleSearchRequest>();
  @Input() startDate: Date | undefined;
  @Input() endDate: Date | undefined;
  @Input() today: Date | undefined;
  @Input() dictionaryListSites: Record<string, string> | undefined;
  @Input() listSitesSelected: string[] | undefined;
  @Input() newDomain: string | undefined;
  @Input() isLoading: boolean | undefined;

  @Output() search = new EventEmitter<void>();
  @Output() updateSites = new EventEmitter<Event>();
  @Output() addDomain = new EventEmitter<void>();

  relatedKeys: string[] = [];
  relatedKeyInput: string = '';
  ignoreTexts: string[] = [];
  ignoreTextInput: string = '';

  isListening = false;
  voiceStatus = false;
  voiceError: string | null = null;

  @ViewChild('voiceIcon') voiceIcon!: ElementRef;

  private recognition: any = null;

  constructor() {
    this.initSpeechRecognition();
  }

  initSpeechRecognition() {
    const SpeechRecognition =
      (window as any).SpeechRecognition ||
      (window as any).webkitSpeechRecognition;
    if (SpeechRecognition) {
      this.recognition = new SpeechRecognition();
      this.recognition.continuous = false;
      this.recognition.interimResults = false;
      this.recognition.lang = 'vi-VN';

      this.recognition.onstart = () => {
        this.isListening = true;
        this.voiceStatus = true;
        this.voiceError = null;
        if (this.voiceIcon) {
          this.voiceIcon.nativeElement.classList.add('text-red-500');
        }
      };

      this.recognition.onresult = (event: any) => {
        const transcript = event.results[0][0].transcript;
        this.updateSearchParameters({ q: transcript });
        this.stopListening();
      };

      this.recognition.onerror = (event: any) => {
        this.voiceError = 'Lỗi ghi âm. Vui lòng thử lại.';
        this.stopListening();
        setTimeout(() => {
          this.voiceError = null;
          this.voiceStatus = false;
        }, 3000);
      };

      this.recognition.onend = () => {
        this.stopListening();
      };
    } else {
      this.voiceError = 'Trình duyệt của bạn không hỗ trợ ghi âm giọng nói.';
      this.voiceStatus = true;
      setTimeout(() => {
        this.voiceError = null;
        this.voiceStatus = false;
      }, 3000);
    }
  }

  stopListening() {
    this.isListening = false;
    if (this.voiceIcon) {
      this.voiceIcon.nativeElement.classList.remove('text-red-500');
    }
  }

  toggleVoiceRecognition() {
    if (!this.recognition) {
      this.voiceError = 'Trình duyệt của bạn không hỗ trợ ghi âm giọng nói.';
      this.voiceStatus = true;
      setTimeout(() => {
        this.voiceError = null;
        this.voiceStatus = false;
      }, 3000);
      return;
    }
    if (this.isListening) {
      this.recognition.stop();
    } else {
      this.recognition.start();
    }
  }

  addRelatedKey() {
    if (this.relatedKeyInput.trim().length === 0) return;
    this.relatedKeys.push(this.relatedKeyInput.trim());
    this.relatedKeyInput = '';
    this.transformRelatedKeys();
  }

  deleteRelatedKey(key: string) {
    this.relatedKeys = this.relatedKeys.filter((x) => x !== key);
    this.transformRelatedKeys();
  }

  clearRelatedKey() {
    this.relatedKeys = [];
    this.updateSearchParameters({ as_oq: '' });
  }

  transformRelatedKeys() {
    const as_oq = this.relatedKeys.map((x) => `"${x}"`).join(' OR ');
    this.updateSearchParameters({ as_oq });
  }

  addIgnoreText() {
    if (this.ignoreTextInput.trim().length === 0) {
      return;
    }
    this.ignoreTexts.push(this.ignoreTextInput.trim());
    this.ignoreTextInput = '';
    this.transformIgnoreTexts();
  }

  deleteIgnoreText(text: string) {
    this.ignoreTexts = this.ignoreTexts.filter((x) => x !== text);
    this.transformIgnoreTexts();
  }

  clearIgnoreText() {
    this.ignoreTexts = [];
    this.updateSearchParameters({ as_eq: '' });
  }

  transformIgnoreTexts() {
    const as_eq = '-' + this.ignoreTexts.map((x) => `"${x}"`).join(' -');
    this.updateSearchParameters({ as_eq });
  }

  updateSearchParameters(params: Partial<SearchRequest>) {
    if (this.searchParameters) {
      this.searchParametersChange.emit({ ...this.searchParameters, ...params });
    }
  }

  isSelected(site: string): boolean {
    return this.listSitesSelected?.some((x) => x === site) ?? false;
  }

  onSearch() {
    this.search.emit();
  }

  onUpdateSites(event: Event) {
    this.updateSites.emit(event);
  }

  onAddDomain() {
    this.addDomain.emit();
  }
}
