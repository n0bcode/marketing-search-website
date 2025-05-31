import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-search-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './search-form.component.html',
})
export class SearchFormComponent {
  @Input() searchQuery!: string;
  @Input() searchParameters!: any;
  @Input() searchNum!: number;
  @Input() startDate!: Date;
  @Input() endDate!: Date;
  @Input() today!: Date;
  @Input() dictionaryListSites!: { [key: string]: string };
  @Input() newDomain!: string;
  @Input() isLoading!: boolean;
  @Output() search = new EventEmitter<void>();
  @Output() updateSites = new EventEmitter<Event>();
  @Output() addDomain = new EventEmitter<void>();

  isSelected(siteKey: string): boolean {
    // Logic để kiểm tra site được chọn
    return false; // Thay bằng logic thực tế
  }
}
