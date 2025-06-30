import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ModalAnalysisMediaSocialComponent } from '../modal-analysis-media-social/modal-analysis-media-social.component';

import * as XLSX from 'xlsx';
import * as FileSaver from 'file-saver';

@Component({
  selector: 'app-search-results',
  standalone: true,
  imports: [CommonModule, ModalAnalysisMediaSocialComponent],
  templateUrl: './search-results.component.html',
})
export class SearchResultsComponent {
  @Input() searchResultsList!: any[];
  @Input() selectedSite!: string;
  @Input() searchResults!: any;
  @Input() isLoading!: boolean;
  @Input() analysisLink!: any;
  @Input() mainDataAnalysisLinkSocialVideo!: any;
  @Input() isShowModal!: boolean;
  @Input() isLoadingDataForModal!: boolean;
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
  // #region [Xử lí xuất dữ liệu tìm kiếm ra file excel]
  exportToExcel(): void {
    // Chọn dữ liệu muốn xuất, ví dụ: this.searchResultsList hoặc this.keywordModels
    const data = this.searchResultsList.map((item) => ({
      'Từ khóa': item.siteSearch,
      'Kết quả': item.showText,
      // Thêm các trường khác nếu muốn
    }));

    // Tạo worksheet và workbook
    const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(data);
    const workbook: XLSX.WorkBook = {
      Sheets: { 'Kết quả tìm kiếm': worksheet },
      SheetNames: ['Kết quả tìm kiếm'],
    };

    // Xuất file
    const excelBuffer: any = XLSX.write(workbook, {
      bookType: 'xlsx',
      type: 'array',
    });
    const blob: Blob = new Blob([excelBuffer], {
      type: 'application/octet-stream',
    });
    FileSaver.saveAs(blob, 'ket-qua-tim-kiem.xlsx');
  }
  // #endregion
}
