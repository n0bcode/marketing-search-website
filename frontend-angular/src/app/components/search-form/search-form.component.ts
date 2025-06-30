import { Component, Input, Output, EventEmitter, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { GoogleSearchRequest } from '../../interfaces/googleSearchService/google-search-request';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-search-form',
  standalone: true,
  imports: [CommonModule, FormsModule, MatProgressSpinnerModule],
  templateUrl: './search-form.component.html',
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

  addRelatedKey() {
    if (this.relatedKeyInput.trim().length === 0) return;
    this.relatedKeys.push(this.relatedKeyInput.trim());
    this.transformRelatedKeys();
  }

  deleteRelatedKey(key: string) {
    this.relatedKeys = this.relatedKeys.filter((x) => x !== key);
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

  updateSearchParameters(params: Partial<GoogleSearchRequest>) {
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
