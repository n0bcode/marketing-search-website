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
    const generalData = this.searchResultsList.map((item) => ({
      'Nền tảng tìm kiếm': item.siteSearch || '',
      'Tiêu đề phân tích': this.getAnalysisTitle(item),
      'Nguồn dữ liệu': this.getAnalysisSource(item),
      'Đánh giá chung': this.getGeneralEvaluation(item),
      'Số bài viết tích cực': this.getPositiveArticles(item).length,
      'Số bài viết tiêu cực': this.getNegativeArticles(item).length,
      'Thông báo không có dữ liệu': this.getNoDataMessage(item),
    }));

    const positiveArticlesData: any[] = [];
    this.searchResultsList.forEach((item) => {
      const siteSearch = item.siteSearch || '';
      this.getPositiveArticles(item).forEach((article) => {
        positiveArticlesData.push({
          'Nền tảng tìm kiếm': siteSearch,
          'Tóm tắt bài viết': article.tomTat,
          'Lý do': article.lyDo,
          'Liên kết': article.lienKet,
        });
      });
    });

    const negativeArticlesData: any[] = [];
    this.searchResultsList.forEach((item) => {
      const siteSearch = item.siteSearch || '';
      this.getNegativeArticles(item).forEach((article) => {
        negativeArticlesData.push({
          'Nền tảng tìm kiếm': siteSearch,
          'Tóm tắt bài viết': article.tomTat,
          'Lý do': article.lyDo,
          'Liên kết': article.lienKet,
        });
      });
    });

    const workbook: XLSX.WorkBook = { Sheets: {}, SheetNames: [] };

    if (generalData.length > 0) {
      const generalWorksheet: XLSX.WorkSheet =
        XLSX.utils.json_to_sheet(generalData);
      XLSX.utils.book_append_sheet(
        workbook,
        generalWorksheet,
        'Kết quả tìm kiếm tổng quan'
      );
    }

    if (positiveArticlesData.length > 0) {
      const positiveWorksheet: XLSX.WorkSheet =
        XLSX.utils.json_to_sheet(positiveArticlesData);
      XLSX.utils.book_append_sheet(
        workbook,
        positiveWorksheet,
        'Bài viết tích cực'
      );
    }

    if (negativeArticlesData.length > 0) {
      const negativeWorksheet: XLSX.WorkSheet =
        XLSX.utils.json_to_sheet(negativeArticlesData);
      XLSX.utils.book_append_sheet(
        workbook,
        negativeWorksheet,
        'Bài viết tiêu cực'
      );
    }

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
