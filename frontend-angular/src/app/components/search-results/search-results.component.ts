import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ModalAnalysisMediaSocialComponent } from '../modal-analysis-media-social/modal-analysis-media-social.component';
import { GeminiResponse } from '../../interfaces/geminiAiService/gemini-response';
import { AnalysisLink } from '../../models/analysis-link';

import * as XLSX from 'xlsx';
import * as FileSaver from 'file-saver';

@Component({
  selector: 'app-search-results',
  standalone: true,
  imports: [CommonModule, ModalAnalysisMediaSocialComponent],
  templateUrl: './search-results.component.html',
})
export class SearchResultsComponent {
  @Input() searchResultsList: GeminiResponse[] = [];
  @Input() selectedSite: string = '';
  @Input() searchResults: GeminiResponse | null = null;
  @Input() isLoading: boolean = false;
  @Input() analysisLink: AnalysisLink | null = null;
  @Input() mainDataAnalysisLinkSocialVideo: GeminiResponse | null = null;
  @Input() isShowModal: boolean = false;
  @Input() isLoadingDataForModal: boolean = false;

  @Output() selectSite = new EventEmitter<string>();
  @Output() analyzeLink = new EventEmitter<string>();
  @Output() analyzeVideo = new EventEmitter<string>();

  isResponseOfSiteHaveValue(site: string): boolean {
    return (
      this.searchResultsList.find((x) => x.siteSearch == site)?.showText == null
    );
  }

  isInSeviceSupport(link: string): boolean {
    const supportServices: string[] = ['tiktok.com', 'facebook.com'];
    return supportServices.some((ss) => link.includes(ss));
  }

  exportToExcel(): void {
    const data = this.searchResultsList.map((item) => ({
      'Từ khóa': item.siteSearch,
      'Kết quả': item.showText,
    }));

    const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(data);
    const workbook: XLSX.WorkBook = {
      Sheets: { 'Kết quả tìm kiếm': worksheet },
      SheetNames: ['Kết quả tìm kiếm'],
    };

    const excelBuffer: any = XLSX.write(workbook, {
      bookType: 'xlsx',
      type: 'array',
    });
    const blob: Blob = new Blob([excelBuffer], {
      type: 'application/octet-stream',
    });
    FileSaver.saveAs(blob, 'ket-qua-tim-kiem.xlsx');
  }
}
