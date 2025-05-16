import { Component, signal } from '@angular/core';
import { ApiService } from '../../utils/http-client-config';
import { GoogleSearchRequest } from '../../interfaces/googleSearchService/google-search-request';
import { GeminiResponse } from '../../interfaces/geminiAiService/gemini-response';
import { MarkdownItConfig } from '../../utils/markdown-it-config';
import ConfigsRequest from '../../models/configs-request';
import { AnalysisLink } from '../../models/analysis-link';
import { CommonModule } from '@angular/common';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { KeywordModel } from '../../models/keyword-model';
import { ResponseAPI } from '../../models/response-api';
import { MatIcon, MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-google-search',
  imports: [CommonModule, MatInputModule, FormsModule, MatIcon, MatIconModule],
  templateUrl: './google-search.component.html',
  styleUrls: ['./google-search.component.css'],
})
export class GoogleSearchComponent {
  isLoading = signal(false);
  searchQuery: string = '';
  searchNum: number = 10;
  searchResults: GeminiResponse | null = null;
  searchResultsList: GeminiResponse[] = []; // Mảng lưu trữ kết quả từ nhiều site
  selectedSite: string = ''; // Lưu site đang được chọn để hiển thị
  analysisLink: AnalysisLink | null = null;
  waitAnalysisLink: AnalysisLink = {
    id: 0,
    link: '',
    analysisText: 'Vui lòng chờ phân tích...',
    createdAt: new Date(),
    updatedAt: new Date(),
  };
  analysisLinks: AnalysisLink[] = [];

  errorMessageResponse: string = '';

  keywordModels: KeywordModel[] | null = null;
  showKeywordHistory: boolean = false;
  storedKeywords: string[] = [];
  listSitesSelected: string[] = [];

  // Khởi tạo `dictionaryListSites`
  dictionaryListSites: Record<string, string> = {
    'facebook.com': 'Facebook',
    'x.com': 'Twitter/X',
    'instagram.com': 'Instagram',
    'linkedin.com': 'LinkedIn',
    'tiktok.com': 'TikTok',
    'reddit.com': 'Reddit',
    'youtube.com': 'YouTube',
  };
  considerSiteAdd: string = '';
  newDomain: string = '';

  searchParameters: GoogleSearchRequest = {
    q: '',
    gl: 'us',
    location: '',
    hl: 'en',
    tbs: '',
    num: 10,
    type: 'search',
    engine: 'google',
    correctPhrase: '',
    anyWords: '',
    notWords: '',
    site: '',
  };

  startDate = new Date();
  endDate = new Date();
  today = new Date().getDate();

  constructor(private apiService: ApiService) {}

  // #region [Tìm kiếm Google]
  onSearch() {
    this.isLoading.set(true);
    this.searchParameters.q = this.searchQuery;
    this.searchParameters.num = this.searchNum;
    this.searchResultsList = [];

    // Định dạng lại ngày tháng theo yêu cầu (MM/DD/YYYY)
    if (this.searchParameters.tbs == 'cdr:1') {
      const tbsValue = `cdr:1,cd_min:${
        this.startDate.getMonth() + 1
      }/${this.startDate.getDate()}/${this.startDate.getFullYear()},cd_max:${
        this.endDate.getMonth() + 1
      }/${this.endDate.getDate()}/${this.endDate.getFullYear()}`;
      this.searchParameters.tbs = tbsValue;
    }

    this.selectedSite = ''; // Reset tab được chọn

    this.storeKeyword(this.searchQuery);

    // Kiểm tra xem có từ khóa nào không
    if (this.listSitesSelected.length === 0) {
      this.performSearch({ ...this.searchParameters, site: '' });
    }
    // Nếu có từ khóa, thực hiện tìm kiếm cho từng site đã chọn
    else {
      this.listSitesSelected.forEach((site) => {
        this.performSearch({ ...this.searchParameters, site });
      });
    }
  }

  // Hàm thực hiện tìm kiếm cho một bộ tham số
  private performSearch(params: GoogleSearchRequest) {
    this.apiService
      .postToApi<ResponseAPI<GeminiResponse>>(
        '/Analysis/SearchGoogleAndAnalysis',
        params,
        ConfigsRequest.getSkipAuthConfig()
      )
      .subscribe({
        next: (response) => {
          // Kiểm tra xem response có thành công không
          if (!(response.success || response.data)) {
            this.errorMessageResponse = response.message;
            this.searchResults = null; // Reset kết quả
            return; // Nếu có lỗi, dừng lại tại đây
          }
          // Kiểm tra xem response có chứa dữ liệu không

          response.data!.showText =
            response.data!.candidates[0].content.parts[0].text;
          /* response.data!.showText = MarkdownItConfig.formatMessageMarkToHtml(
            response.data!.candidates[0].content.parts[0].text
          ); */
          // Lưu kết quả vào response
          response.data!.siteSearch = params.site || 'default'; // Lưu site vào response
          response.data!.generalSearchResultsCount =
            response.data!.generalSearchResults.length;
          this.searchResultsList.push(response.data!);
          // Nếu chưa có site nào được chọn, chọn site đầu tiên
          if (!this.selectedSite) {
            this.selectedSite = response.data!.siteSearch; // Chọn site đầu tiên mặc định
            this.searchResults = response.data!;
          }
          // Nếu đã có site được load thành công, đưa dữ liệu ra ngoài hiển thị
          this.isLoading.set(false);
          console.log(`Result for site ${params.site || 'default'}:`, response);
        },
        error: (err) => {
          console.error(
            `Error fetching search results for site ${
              params.site || 'default'
            }:`,
            err
          );
          this.isLoading.set(
            this.searchResultsList.length < this.listSitesSelected.length
          );
        },
      });
  }
  // #endregion

  // #region [Phân tích liên kết]
  onAnalysisLink(link: string) {
    const existingAnalysis = this.analysisLinks.find((a) => a.link == link);
    if (existingAnalysis) {
      this.analysisLink = existingAnalysis;
      return;
    } else {
      this.analysisLink = this.waitAnalysisLink; // Reset phân tích liên kết trước đó
    }

    this.apiService
      .postToApi<AnalysisLink>(
        `/Analysis/AnalysisLink?link=${link}`,
        '',
        ConfigsRequest.getSkipAuthConfig()
      )
      .subscribe({
        next: (response) => {
          response.analysisText = MarkdownItConfig.formatMessageMarkToHtml(
            response.analysisText
          );
          this.analysisLink = response;
          this.analysisLinks.push(response);
        },
        error: (err) => {
          this.errorMessageResponse =
            'Đã xảy ra lỗi khi phân tích liên kết: ' +
            (err.message || 'Lỗi không xác định');
          console.error('Error fetching analysis link:', err);
        },
      });
  }

  // #endregion

  // #region [Phương thức để lưu từ khóa vào lịch sử]
  storeKeyword(keyword: string) {
    if (!this.keywordModels) {
      this.keywordModels = [];
    }

    const newKeyword: KeywordModel = {
      id: Date.now().toString(),
      keyword: keyword,
      relatedKeyword: '',
      source: '',
      term: '',
      socialMediaInfo: '',
      createdAt: new Date(),
      updatedAt: new Date(),
      note: '',
    };

    this.keywordModels.push(newKeyword);
  }
  // #endregion

  // #region [Phương thức để ẩn/hiện lịch sử từ khóa]
  toggleKeywordHistory() {
    this.showKeywordHistory = !this.showKeywordHistory;
  }
  // #endregion

  // #region [Lấy danh sách từ khóa từ API]
  onTakeKeywordGoogle() {
    this.apiService
      .getFromApi<ResponseAPI<KeywordModel[]>>(
        '/Search/GetAllKeyword',
        ConfigsRequest.getSkipAuthConfig()
      )
      .subscribe({
        next: (response) => {
          this.keywordModels = response.data;
          console.log(this.keywordModels);
        },
        error: (err) => {
          console.error('Error fetching search results:', err);
        },
      });
  }
  // #endregion

  // #region [Lấy danh sách từ khóa từ API]
  loadOldAnalysis(keywordId: string) {
    console.log(keywordId);
    this.apiService
      .getFromApi<GeminiResponse[]>(
        `/Analysis/GetOldAnalysis/${keywordId}`,
        ConfigsRequest.getSkipAuthConfig()
      )
      .subscribe({
        next: (response) => {
          this.searchResults = response[0];
          console.log(this.searchResults);
          this.processSearchResults(this.searchResults);
        },
        error: (err) => {
          console.error('Error fetching search results:', err);
        },
      });
  }
  // #endregion

  // #region [Phương thức để xử lý kết quả tìm kiếm]
  processSearchResults(response: GeminiResponse): void {
    console.log(response);
    if (
      response &&
      Array.isArray(response.candidates) &&
      response.candidates.length > 0
    ) {
      let combinedText = '';

      for (const candidate of response.candidates) {
        if (candidate.content && Array.isArray(candidate.content.parts)) {
          for (const part of candidate.content.parts) {
            combinedText += part.text;
          }
        }
      }

      response.showText =
        MarkdownItConfig.formatMessageMarkToHtml(combinedText);
    } else {
      response.showText = 'Không có dữ liệu tìm kiếm nào được tìm thấy.';
    }
  }
  // #endregion

  // #region [Hàm xử lý khi checkbox thay đổi trạng thái]
  updateSelectedSites(event: Event): void {
    const input = event.target as HTMLInputElement;
    const site = input.value;

    if (input.checked) {
      if (!this.listSitesSelected.includes(site)) {
        this.listSitesSelected.push(site);
      }
    } else {
      this.listSitesSelected = this.listSitesSelected.filter((s) => s !== site);
    }

    console.log('Selected sites:', this.listSitesSelected);
  }
  // #endregion
  // #region [Chọn site để hiển thị kết quả]
  selectSite(site: string) {
    this.selectedSite = site;
    const selectedResult = this.searchResultsList.find(
      (result) => result.siteSearch === site
    );
    this.searchResults = selectedResult || null;
  }
  // #endregion
  // #region [Hàm để lấy danh sách các site đã chọn]
  getSelectedSites(): string[] {
    return this.listSitesSelected;
  }
  // #endregion

  // #region [Kiểm tra xem site đã được chọn hay chưa]
  isSelected(site: string): boolean {
    return this.listSitesSelected.some((x) => x == site);
  }
  // #endregion

  addDomainToDictionary() {
    if (this.newDomain && !this.dictionaryListSites[this.newDomain]) {
      // Thêm tên miền vào dictionaryListSites với giá trị mặc định (có thể tùy chỉnh)
      this.dictionaryListSites[this.newDomain] = this.newDomain; // Hoặc một giá trị mô tả khác

      // Reset ô nhập
      this.newDomain = '';
      console.log(
        'Tên miền đã được thêm vào từ điển:',
        this.dictionaryListSites
      );
    } else if (this.dictionaryListSites[this.newDomain]) {
      console.log('Tên miền đã tồn tại trong từ điển.');
    } else {
      console.log('Vui lòng nhập tên miền hợp lệ.');
    }
  }

  isResponseOfSiteHaveValue(site: string): boolean {
    return (
      this.searchResultsList.find((x) => x.siteSearch == site)?.showText == null
    );
  }
}
