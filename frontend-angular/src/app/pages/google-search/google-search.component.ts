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
  analysisLinks: AnalysisLink[] = [];

  keywordModels: KeywordModel[] | null = null;
  showKeywordHistory: boolean = false;
  storedKeywords: string[] = [];
  listSitesSelected: string[] = [];

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

  constructor(private apiService: ApiService) {}

  // #region [Tìm kiếm Google]
  onSearch() {
    this.searchParameters.q = this.searchQuery;
    this.searchParameters.num = this.searchNum;
    this.isLoading.set(true);
    this.searchResultsList = [];
    this.selectedSite = ''; // Reset tab được chọn

    this.storeKeyword(this.searchQuery);

    if (this.listSitesSelected.length === 0) {
      this.performSearch({ ...this.searchParameters, site: '' });
    } else {
      this.listSitesSelected.forEach((site) => {
        this.performSearch({ ...this.searchParameters, site });
      });
    }
  }

  // Hàm thực hiện tìm kiếm cho một bộ tham số
  private performSearch(params: GoogleSearchRequest) {
    this.apiService
      .postToApi<GeminiResponse>(
        '/Analysis/SearchGoogleAndAnalysis',
        params,
        ConfigsRequest.getSkipAuthConfig()
      )
      .subscribe({
        next: (response) => {
          response.showText = MarkdownItConfig.formatMessageMarkToHtml(
            response.candidates[0].content.parts[0].text
          );
          response.siteSearch = params.site || 'default'; // Lưu site vào response
          this.searchResultsList.push(response);
          if (!this.selectedSite) {
            this.selectedSite = response.siteSearch; // Chọn site đầu tiên mặc định
            this.searchResults = response;
          }
          this.isLoading.set(
            this.searchResultsList.length < this.listSitesSelected.length
          );
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
}
