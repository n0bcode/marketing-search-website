import {
  Component,
  signal,
  computed,
  Signal,
  ViewChild,
  ElementRef,
} from '@angular/core';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ApiService } from '../../utils/http-client-config';
import { GoogleSearchRequest } from '../../interfaces/googleSearchService/google-search-request';
import { GeminiResponse } from '../../interfaces/geminiAiService/gemini-response'; // Import Candidate
import ConfigsRequest from '../../models/configs-request';
import { MarkdownItConfig } from '../../utils/markdown-it-config';
import { TwitterSearchTweetRequest } from '../../interfaces/twitterSearchService/request/twitter-search-tweet-request';
import { AnalysisLink } from '../../models/analysis-link';

@Component({
  selector: 'app-tests',
  standalone: true,
  imports: [
    MatInputModule,
    MatSelectModule,
    MatIconModule,
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
  ],
  providers: [ApiService],
  templateUrl: './tests.component.html',
  styleUrls: ['./tests.component.css'],
})
export class TestsComponent {
  isGoogleLoading = signal(false); // Biến để xác định trạng thái tải dữ liệu
  isTwitterLoading = signal(false); // Biến để xác định trạng thái tải dữ liệu twitter

  // #region [MAIN CODE]
  title = 'TestsComponent';

  // Biến tạm để lưu giá trị tìm kiếm
  searchQuery: string = '';
  searchNum: number = 10;

  analysisLink: AnalysisLink | null = null;
  analysisLinks: AnalysisLink[] = []; // Mảng lưu trữ thông tin phân tích

  searchResults: GeminiResponse | null = null; // Cập nhật kiểu dữ liệu
  twitterSearchResults: GeminiResponse | null = null; // Cập nhật kiểu dữ liệu twitter
  lastTimeRequetTwitter: Date = new Date(); // Thời gian yêu cầu cuối cùng
  timeoutWaitRequestTwitter = computed(() => {
    this.lastTimeRequetTwitter.getTime() <=
      this.lastTimeRequetTwitter.getTime() - 1000 * 60 * 15; // 15 phút
  });
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
  twitterSearchParameters: TwitterSearchTweetRequest = {
    query: '',
    max_results: 10,
    tweet_fields: [],
    expansions: [],
    user_fields: [],
    start_time: '',
    end_time: '',
    since_id: '',
    until_id: '',
    pagination_token: '',
    sort_order: '',
    media_fields: [],
    poll_fields: [],
    place_fields: [],
  };
  isShowSearchBar = signal(false);
  isShowSearchResult = signal(false);
  isDetailMode = false; // Biến để xác định chế độ chi tiết hay không
  activeTab: 'google' | 'twitter' = 'google';

  constructor(private apiService: ApiService) {}

  // Phương thức để hiển thị thanh tìm kiếm
  showSearchBar() {
    this.isShowSearchBar.set(true);
  }

  // Phương thức để ẩn thanh tìm kiếm
  hideSearchBar() {
    this.isShowSearchBar.set(false);
  }

  // Phương thức để hiển thị kết quả tìm kiếm
  showSearchResult() {
    this.isShowSearchResult.set(true);
  }

  // Phương thức để ẩn kết quả tìm kiếm
  hideSearchResult() {
    this.isShowSearchResult.set(false);
  }

  setActiveTab(tab: 'google' | 'twitter') {
    this.activeTab = tab;
  }

  toggleDetailMode() {
    this.isDetailMode = !this.isDetailMode; // Chuyển đổi giữa chế độ chi tiết và cơ bản
  }

  testMethod() {
    console.log('Test method called!');
    this.apiService
      .getFromApi<string>(
        '/Analysis/GetGood',
        ConfigsRequest.getSkipAuthConfig()
      )
      .subscribe((response) => {
        console.log('Response:', response);
        alert('Response: ' + JSON.stringify(response)); // Hiển thị thông báo với dữ liệu JSON
      });
  }

  // Phương thức để xử lý sự kiện khi người dùng nhấn nút tìm kiếm
  onSearch() {
    this.searchParameters.q = this.searchQuery; // Gán giá trị từ biến tạm
    this.searchParameters.num = this.searchNum; // Gán số lượng từ biến tạm
    this.searchResults = null;
    this.hideSearchResult();

    this.isGoogleLoading.set(true); // Bắt đầu tải dữ liệu

    // Gọi API tìm kiếm
    this.apiService
      .postToApi<GeminiResponse>(
        '/Analysis/SearchGoogleAndAnalysis',
        this.searchParameters,
        ConfigsRequest.getSkipAuthConfig()
      )
      .subscribe({
        next: (response) => {
          this.searchResults = response; // Lưu trực tiếp candidates vào searchResults
          this.searchResults.showText =
            MarkdownItConfig.formatMessageMarkToHtml(
              response.candidates[0].content.parts[0].text
            ); // Chuyển đổi nội dung sang HTML
          console.log(this.searchResults); // In ra kết quả tìm kiếm

          this.showSearchResult(); // Hiển thị kết quả tìm kiếm
          this.hideSearchBar(); // Ẩn thanh tìm kiếm
          this.isGoogleLoading.set(false); // Kết thúc tải dữ liệu
        },
        error: (err) => {
          console.error('Error fetching search results:', err);
          this.isGoogleLoading.set(false); // Kết thúc tải dữ liệu
        },
      });
  }
  onSearchTwitter() {
    this.twitterSearchParameters.query = this.searchQuery; // Gán giá trị từ biến tạm
    this.twitterSearchParameters.max_results = this.searchNum; // Gán số lượng từ biến tạm

    this.isTwitterLoading.set(true); // Bắt đầu tải dữ liệu

    // Gọi API tìm kiếm Twitter
    this.apiService
      .postToApi<GeminiResponse>(
        '/Analysis/SearchTwitterAndAnalysis',
        this.twitterSearchParameters,
        ConfigsRequest.getSkipAuthConfig()
      )
      .subscribe({
        next: (response) => {
          this.twitterSearchResults = response; // Lưu trực tiếp candidates vào searchResults
          this.twitterSearchResults.showText =
            MarkdownItConfig.formatMessageMarkToHtml(
              response.candidates[0].content.parts[0].text
            ); // Chuyển đổi nội dung sang HTML
          console.log(this.twitterSearchResults); // In ra kết quả tìm kiếm

          this.showSearchResult(); // Hiển thị kết quả tìm kiếm
          this.hideSearchBar(); // Ẩn thanh tìm kiếm
          this.isTwitterLoading.set(false); // Kết thúc tải dữ liệu
          this.lastTimeRequetTwitter = new Date(); // Cập nhật thời gian yêu cầu cuối cùng
        },
        error: (err) => {
          console.error('Error fetching search results:', err);
          this.isTwitterLoading.set(false); // Kết thúc tải dữ liệu
        },
      });
  }

  onAnalysisLink(link: string) {
    // Kiểm tra xem thông tin phân tích đã có trong mảng chưa
    const existingAnalysis = this.analysisLinks.find((a) => a.link == link);
    if (existingAnalysis) {
      this.analysisLink = existingAnalysis; // Sử dụng thông tin đã có
      console.log(this.analysisLink);
      return; // Không cần gửi yêu cầu API
    }

    // Nếu không có, gửi yêu cầu API
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
          this.analysisLinks.push(response); // Lưu thông tin vào mảng
        },
        error: (err) => {
          console.error('Error fetching analysis link:', err);
        },
      });
    console.log(this.analysisLink);
  }
  // #endregion
  // #region [HTML CODE]

  // #endregion Phương thức để xử lý sự kiện khi người dùng nhấn nút "Xem chi tiết"
}
