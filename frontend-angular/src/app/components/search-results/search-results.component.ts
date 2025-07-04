import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ModalAnalysisMediaSocialComponent } from '../modal-analysis-media-social/modal-analysis-media-social.component';
import {
  AnalysisResult,
  Article,
  ArticleEvaluation,
  GeminiResponse,
} from '../../interfaces/geminiAiService/gemini-response';
import { AnalysisLink } from '../../models/analysis-link';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import * as XLSX from 'xlsx';
import * as FileSaver from 'file-saver';

@Component({
  selector: 'app-search-results',
  standalone: true,
  imports: [
    CommonModule,
    ModalAnalysisMediaSocialComponent,
    MatProgressSpinnerModule,
  ],
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
  @Input() isAnalyzingLink: boolean = false;
  @Input() error: string | null = null;

  @Output() selectSite = new EventEmitter<string>();
  @Output() analyzeLink = new EventEmitter<string>();
  @Output() analyzeVideo = new EventEmitter<string>();
  @Output() modalToggle = new EventEmitter<void>();

  isResponseOfSiteHaveValue(site: string): boolean {
    const result = this.searchResultsList.find((x) => x.siteSearch == site);
    return result != null && result.analysisData != null;
  }

  getAnalysisTitle(response: GeminiResponse | null): string {
    return response?.analysisData?.tieuDe || 'Không có tiêu đề';
  }

  getAnalysisSource(response: GeminiResponse | null): string {
    return response?.analysisData?.nguonDuLieu || 'Không rõ nguồn';
  }

  getGeneralEvaluation(response: GeminiResponse | null): string {
    return (
      response?.analysisData?.danhGia?.danhGiaChung ||
      'Không có đánh giá chung.'
    );
  }

  getPositiveArticles(response: GeminiResponse | null): Article[] {
    return response?.analysisData?.danhGia?.tichCuc?.baiViet || [];
  }

  getNegativeArticles(response: GeminiResponse | null): Article[] {
    return response?.analysisData?.danhGia?.tieuCuc?.baiViet || [];
  }

  getNoDataMessage(response: GeminiResponse | null): string {
    return (
      response?.analysisData?.thongBaoKhongCoDuLieu ||
      'Không tìm thấy dữ liệu liên quan.'
    );
  }

  isInSeviceSupport(link: string): boolean {
    const supportServices: string[] = ['tiktok.com', 'facebook.com'];
    return supportServices.some((ss) => link.includes(ss));
  }

  exportToExcel(): void {
    const data = this.searchResultsList.map((item) => ({
      'Từ khóa': item.siteSearch || '',
      'Tiêu đề phân tích': this.getAnalysisTitle(item),
      'Nguồn dữ liệu': this.getAnalysisSource(item),
      'Đánh giá chung': this.getGeneralEvaluation(item),
      'Bài viết tích cực': this.getPositiveArticles(item)
        .map((a) => `${a.tomTat} (${a.lienKet})`)
        .join('; '),
      'Bài viết tiêu cực': this.getNegativeArticles(item)
        .map((a) => `${a.tomTat} (${a.lienKet})`)
        .join('; '),
      'Thông báo không có dữ liệu': this.getNoDataMessage(item),
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

  onModalToggle() {
    this.modalToggle.emit();
  }
}
