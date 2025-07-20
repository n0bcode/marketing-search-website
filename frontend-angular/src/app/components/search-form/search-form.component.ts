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

import { KeywordHistoryService } from '../../services/keyword-history.service';

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

  isListeningQuery = false;
  voiceStatusQuery = false;
  voiceErrorQuery: string | null = null;
  isListeningExact = false;
  voiceStatusExact = false;
  voiceErrorExact: string | null = null;

  @ViewChild('voiceIcon') voiceIcon!: ElementRef;
  @ViewChild('voiceIconExact') voiceIconExact!: ElementRef;

  private recognition: any = null;
  private activeField: 'query' | 'exact' | null = null;

  constructor(private keywordHistoryService: KeywordHistoryService) {
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
        if (this.activeField === 'query') {
          this.isListeningQuery = true;
          this.voiceStatusQuery = true;
          this.voiceErrorQuery = null;
          if (this.voiceIcon) {
            this.voiceIcon.nativeElement.classList.add('text-red-500');
          }
        } else if (this.activeField === 'exact') {
          this.isListeningExact = true;
          this.voiceStatusExact = true;
          this.voiceErrorExact = null;
          if (this.voiceIconExact) {
            this.voiceIconExact.nativeElement.classList.add('text-red-500');
          }
        }
      };

      this.recognition.onresult = (event: any) => {
        const transcript = event.results[0][0].transcript;
        if (this.activeField === 'query') {
          this.updateSearchParameters({ q: transcript });
        } else if (this.activeField === 'exact') {
          this.updateSearchParameters({ as_epq: transcript });
        }
        this.stopListening();
      };

      this.recognition.onerror = (event: any) => {
        const errorMessage = 'Lỗi ghi âm. Vui lòng thử lại.';
        if (this.activeField === 'query') {
          this.voiceErrorQuery = errorMessage;
          this.voiceStatusQuery = true;
        } else if (this.activeField === 'exact') {
          this.voiceErrorExact = errorMessage;
          this.voiceStatusExact = true;
        }
        this.stopListening();
        setTimeout(() => {
          if (this.activeField === 'query') {
            this.voiceErrorQuery = null;
            this.voiceStatusQuery = false;
          } else if (this.activeField === 'exact') {
            this.voiceErrorExact = null;
            this.voiceStatusExact = false;
          }
        }, 3000);
      };

      this.recognition.onend = () => {
        this.stopListening();
      };
    } else {
      const errorMessage = 'Trình duyệt của bạn không hỗ trợ ghi âm giọng nói.';
      this.voiceErrorQuery = errorMessage;
      this.voiceStatusQuery = true;
      this.voiceErrorExact = errorMessage;
      this.voiceStatusExact = true;
      setTimeout(() => {
        this.voiceErrorQuery = null;
        this.voiceStatusQuery = false;
        this.voiceErrorExact = null;
        this.voiceStatusExact = false;
      }, 3000);
    }
  }

  stopListening() {
    if (this.activeField === 'query') {
      this.isListeningQuery = false;
      if (this.voiceIcon) {
        this.voiceIcon.nativeElement.classList.remove('text-red-500');
      }
    } else if (this.activeField === 'exact') {
      this.isListeningExact = false;
      if (this.voiceIconExact) {
        this.voiceIconExact.nativeElement.classList.remove('text-red-500');
      }
    }
    this.activeField = null;
  }

  toggleVoiceRecognition(field: 'query' | 'exact') {
    if (!this.recognition) {
      const errorMessage = 'Trình duyệt của bạn không hỗ trợ ghi âm giọng nói.';
      if (field === 'query') {
        this.voiceErrorQuery = errorMessage;
        this.voiceStatusQuery = true;
      } else {
        this.voiceErrorExact = errorMessage;
        this.voiceStatusExact = true;
      }
      setTimeout(() => {
        if (field === 'query') {
          this.voiceErrorQuery = null;
          this.voiceStatusQuery = false;
        } else {
          this.voiceErrorExact = null;
          this.voiceStatusExact = false;
        }
      }, 3000);
      return;
    }
    if ((field === 'query' && this.isListeningQuery) || (field === 'exact' && this.isListeningExact)) {
      this.recognition.stop();
    } else {
      this.activeField = field;
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
    if (this.searchParameters?.q) {
      this.keywordHistoryService.addKeyword(this.searchParameters.q);
    }
    this.search.emit();
  }

  onUpdateSites(event: Event) {
    this.updateSites.emit(event);
  }

  onAddDomain() {
    this.addDomain.emit();
  }
}

