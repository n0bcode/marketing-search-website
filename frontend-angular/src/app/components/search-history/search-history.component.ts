import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIcon } from '@angular/material/icon';
import { KeywordModel } from '../../models/keyword-model';
import { KeywordHistoryService } from '../../services/keyword-history.service';

@Component({
  selector: 'app-search-history',
  standalone: true,
  imports: [CommonModule, MatIcon],
  templateUrl: './search-history.component.html',
})
export class SearchHistoryComponent implements OnInit {
  @Input() showKeywordHistory: boolean = false;
  @Output() toggleHistory = new EventEmitter<void>();
  @Output() loadAnalysis = new EventEmitter<string>();

  @Input() keywordModels: KeywordModel[] = [];

  constructor(private keywordHistoryService: KeywordHistoryService) {}

  ngOnInit(): void {
    this.loadHistory();
  }

  async loadHistory(): Promise<void> {
    this.keywordModels = await this.keywordHistoryService.getAllKeywords();
  }

  async deleteKeyword(id: any): Promise<void> {
    await this.keywordHistoryService.deleteKeyword(Number(id));
    this.loadHistory(); // Refresh the list
  }

  onToggleHistory(): void {
    if (!this.showKeywordHistory) {
      this.loadHistory(); // Reload history when opening
    }
    this.toggleHistory.emit();
  }
}
