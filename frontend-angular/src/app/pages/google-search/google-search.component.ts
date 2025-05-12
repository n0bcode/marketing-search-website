import { Component, signal, Signal } from '@angular/core';
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
  analysisLink: AnalysisLink | null = null;
  analysisLinks: AnalysisLink[] = [];

  keywordModels: KeywordModel[] | null = null;
  showKeywordHistory: boolean = false; // Biến để theo dõi trạng thái ẩn/hiện lịch sử
  storedKeywords: string[] = []; // Mảng lưu trữ các từ khóa tìm kiếm

  searchParameters: GoogleSearchRequest = {
    q: '',
    gl: 'us',
    location: '',
    hl: 'en',
    tbs: '',
    num: 10,
    type: 'search',
    engine: 'google',
    correctPhrase: '', // Trường này hỗ trợ lọc kết quả tìm kiếm
    anyWords: '', // Trường này hỗ trợ lọc kết quả tìm kiếm
    notWords: '', // Trường này hỗ trợ lọc kết quả tìm kiếm
    site: '', // Trường này hỗ trợ lọc kết quả tìm kiếm
  };

  constructor(private apiService: ApiService) {}

  // Phương thức tìm kiếm Google
  onSearch() {
    this.searchParameters.q = this.searchQuery;
    this.searchParameters.num = this.searchNum;
    this.isLoading.set(true);

    // Lưu từ khóa vào lịch sử
    this.storeKeyword(this.searchQuery);

    this.apiService
      .postToApi<GeminiResponse>(
        '/Analysis/SearchGoogleAndAnalysis',
        this.searchParameters,
        ConfigsRequest.getSkipAuthConfig()
      )
      .subscribe({
        next: (response) => {
          this.searchResults = response;
          this.searchResults.showText =
            MarkdownItConfig.formatMessageMarkToHtml(
              response.candidates[0].content.parts[0].text
            );
          this.isLoading.set(false);
          console.log(this.searchResults);
        },
        error: (err) => {
          console.error('Error fetching search results:', err);
          this.isLoading.set(false);
        },
      });
  }

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

  // Phương thức để lưu từ khóa vào lịch sử
  storeKeyword(keyword: string) {
    if (!this.keywordModels) {
      this.keywordModels = [];
    }

    const newKeyword: KeywordModel = {
      id: Date.now().toString(), // Tạo ID duy nhất cho từ khóa
      keyword: keyword,
      relatedKeyword: '', // Có thể thêm logic liên quan nếu cần
      source: '',
      term: '',
      socialMediaInfo: '',
      createdAt: new Date(),
      updatedAt: new Date(),
      note: '',
    };

    this.keywordModels.push(newKeyword);
  }

  // Phương thức để ẩn/hiện lịch sử từ khóa
  toggleKeywordHistory() {
    this.showKeywordHistory = !this.showKeywordHistory;
  }

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
  } // Đoạn mã trong một hàm (ví dụ: hàm xử lý kết quả tìm kiếm)
  processSearchResults(response: GeminiResponse): void {
    console.log(response);
    if (
      response &&
      Array.isArray(response.candidates) &&
      response.candidates.length > 0
    ) {
      let combinedText = '';

      // Lặp qua từng candidate trong response
      for (const candidate of response.candidates) {
        if (candidate.content && Array.isArray(candidate.content.parts)) {
          // Lặp qua từng part trong Content
          for (const part of candidate.content.parts) {
            combinedText += part.text; // Ghép văn bản của từng part
          }
        }
      }

      // Gán giá trị cho showText sau khi ghép
      this.searchResults!.showText =
        MarkdownItConfig.formatMessageMarkToHtml(combinedText);
    } else {
      // Xử lý trường hợp không có dữ liệu hợp lệ
      this.searchResults!.showText =
        'Không có dữ liệu tìm kiếm nào được tìm thấy.';
    }
  }
}
