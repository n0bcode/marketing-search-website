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
}
