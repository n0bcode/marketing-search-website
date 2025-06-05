import {
  Component,
  EventEmitter,
  Input,
  Output,
  OnChanges,
  SimpleChanges,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { GoogleSearchRequest } from '../../interfaces/googleSearchService/google-search-request';

@Component({
  selector: 'app-search-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './search-form.component.html',
})
export class SearchFormComponent implements OnChanges {
  @Input() searchParameters!: GoogleSearchRequest;
  @Input() startDate!: Date;
  @Input() endDate!: Date;
  @Input() today!: Date;
  @Input() dictionaryListSites!: { [key: string]: string };
  @Input() newDomain!: string;
  @Input() isLoading!: boolean;
  @Input() listSitesSelected!: string[];
  @Output() search = new EventEmitter<Event>();
  @Output() updateSites = new EventEmitter<Event>();
  @Output() addDomain = new EventEmitter<void>();

  // New output for two-way binding of searchParameters
  @Output() searchParametersChange = new EventEmitter<GoogleSearchRequest>();

  // Local copy of searchParameters for two-way binding
  localSearchParameters!: GoogleSearchRequest;

  ngOnChanges(changes: SimpleChanges): void {
    if (
      changes['searchParameters'] &&
      changes['searchParameters'].currentValue
    ) {
      // Create a copy to avoid mutating parent input directly
      this.localSearchParameters = {
        ...changes['searchParameters'].currentValue,
      };
    }
  }

  // Emit changes when any input changes
  onSearchParameterChange() {
    this.searchParametersChange.emit(this.localSearchParameters);
  }

  isSelected(site: string): boolean {
    return this.listSitesSelected.some((x) => x == site);
  }

  // Select related key search
  relatedKeys: string[] = [];
  relatedKeyInput: string = '';
  addRelatedKey() {
    if (this.relatedKeyInput.trim().length == 0) return;
    this.relatedKeys.push(this.relatedKeyInput.trim());
    this.transformRelatedKeys();
  }
  deleteRelatedKey(key: string) {
    this.relatedKeys = this.relatedKeys.filter((x) => x != key);
    this.transformRelatedKeys();
  }
  clearRelatedKey() {
    this.relatedKeys = [];
    this.searchParameters.as_oq = '';
  }
  transformRelatedKeys() {
    this.searchParameters.as_oq = this.relatedKeys
      .map((x) => '"' + x + '"')
      .join(' OR ');
  }
  // Select ignore text search
  ignoreTexts: string[] = [];
  ignoreTextInput: string = '';
  addIgnoreText() {
    if (this.ignoreTextInput.trim().length == 0) {
      return;
    }
    this.ignoreTexts.push(this.ignoreTextInput.trim());
    this.transformIgnoreTexts();
  }
  deleteIgnoreText(text: string) {
    this.ignoreTexts = this.ignoreTexts.filter((x) => x != text);
    this.transformIgnoreTexts();
  }
  clearIgnoreText() {
    this.ignoreTexts = [];
    this.searchParameters.as_eq = '';
  }
  transformIgnoreTexts() {
    this.searchParameters.as_eq =
      '-' + this.ignoreTexts.map((x) => '"' + x + '"').join(' -');
  }
}
