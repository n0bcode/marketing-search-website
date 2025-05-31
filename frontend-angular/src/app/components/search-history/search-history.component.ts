import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIcon } from '@angular/material/icon';

@Component({
  selector: 'app-search-history',
  standalone: true,
  imports: [CommonModule, MatIcon],
  templateUrl: './search-history.component.html',
})
export class SearchHistoryComponent {
  @Input() showKeywordHistory!: boolean;
  @Input() keywordModels!: any[];
  @Output() toggleHistory = new EventEmitter<void>();
  @Output() loadAnalysis = new EventEmitter<string>();
}
