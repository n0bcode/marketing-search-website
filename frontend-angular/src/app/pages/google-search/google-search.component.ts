import { Component, OnInit, signal, ViewChild } from '@angular/core';
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
import { SecretTokenRequestDTO } from '../../models/dtos/secret-token-dto/secret-token-request-dto';
import { TypeServicesConstants } from '../../constants/type-services-constants';
import { SecretTokenResponseDTO } from '../../models/dtos/secret-token-dto/secret-token-response-dto';
import { SecretTokenPanelComponent } from '../../components/secret-token-panel/secret-token-panel.component';
import { SearchResultsComponent } from '../../components/search-results/search-results.component';
import { SearchHistoryComponent } from '../../components/search-history/search-history.component';
import { SearchFormComponent } from '../../components/search-form/search-form.component';

@Component({
  selector: 'app-google-search',
  imports: [
    CommonModule,
    MatInputModule,
    FormsModule,
    MatIconModule,
    SearchFormComponent,
    SecretTokenPanelComponent,
    SearchResultsComponent,
    SearchHistoryComponent,
  ],
  templateUrl: './google-search.component.html',
  styleUrls: ['./google-search.component.css'],
})
export class GoogleSearchComponent implements OnInit {
  ngOnInit(): void {
    this.loadSecretTokens();
    this.listServices.forEach((service) => {
      this.listSelectSecretToken[service] = '';
    });
  }
  // ------ Các biến và trạng thái của component ------

  // #region [Các biến và trạng thái của component]
  isLoading = signal(false);
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

  keywordModels: KeywordModel[] | any[] = [];
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
    as_epq: '',
    anyWords: '',
    notWords: '',
    as_sitesearch: '',
  };

  startDate = new Date();
  endDate = new Date();
  today = new Date();
  // #endregion

  // ------ Secret Token Management ------
  addSecretToken: boolean = false;
  secretTokenDTO: SecretTokenRequestDTO = {
    name: '',
    token: '',
    service: '',
    note: '',
  };
  isShowViewSettingToken: boolean = false;

  listServices: string[] = TypeServicesConstants.ActiveServices;
  listSecretDTOs: SecretTokenResponseDTO[] = [];
  listSecretDTOsMap: Record<string, SecretTokenResponseDTO[]> = {};
  listSelectSecretToken: Record<string, string> = {};

  mainDataAnalysisLinkSocialVideo: GeminiResponse | null = null;
  isShowModal: boolean = false;
  isLoadingDataForModal: boolean = true;

  constructor(private apiService: ApiService) {}

  // #region [Tìm kiếm Google]
  onSearch() {
    this.isLoading.set(true);
    /* 
    this.searchParameters.q = this.searchQuery;
    this.searchParameters.num = this.searchNum; */
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

    this.storeKeyword(this.searchParameters.q);

    // Kiểm tra xem có từ khóa nào không
    if (this.listSitesSelected.length === 0) {
      this.performSearch({ ...this.searchParameters, as_sitesearch: '' });
    }
    // Nếu có từ khóa, thực hiện tìm kiếm cho từng site đã chọn
    else {
      this.listSitesSelected.forEach((site) => {
        this.performSearch({ ...this.searchParameters, as_sitesearch: site });
      });
    }
  }

  // Hàm thực hiện tìm kiếm cho một bộ tham số
  private performSearch(params: GoogleSearchRequest) {
    this.apiService
      .postToApi<ResponseAPI<GeminiResponse>>(
        `/Analysis/SearchGoogleAndAnalysis?idTokenGoogleChange=${
          this.listSelectSecretToken[TypeServicesConstants.GoogleSearch]
        }&idTokenGeminiChange=${
          this.listSelectSecretToken[TypeServicesConstants.GeminiAI]
        }`,
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
            response.data!.candidates[0].content.parts[0].text.replace(
              /```html|```/g,
              ''
            );
          /* response.data!.showText = MarkdownItConfig.formatMessageMarkToHtml(
            response.data!.candidates[0].content.parts[0].text
          ); */
          // Lưu kết quả vào response
          response.data!.siteSearch = params.as_sitesearch || 'default'; // Lưu site vào response
          response.data!.generalSearchResultsCount =
            response.data!.generalSearchResults.length;
          this.searchResultsList.push(response.data!);
          // Nếu chưa có site nào được chọn, chọn site đầu tiên
          if (!this.selectedSite) {
            this.selectedSite = response.data!.siteSearch; // Chọn site đầu tiên mặc định
            this.searchResults = response.data!;
          }
        },
        error: (err) => {
          console.error(
            `Error fetching search results for site ${
              params.as_sitesearch || 'default'
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
        `/Analysis/AnalysisLink?link=${encodeURIComponent(
          link
        )}&idTokenChange=${
          this.listSelectSecretToken[TypeServicesConstants.GeminiAI]
        }`,
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
          this.keywordModels = response.data!; // ? Not good set
        },
        error: (err) => {
          console.error('Error fetching search results:', err);
        },
      });
  }
  // #endregion

  // #region [Lấy danh sách từ khóa từ API]
  loadOldAnalysis(keywordId: string) {
    this.apiService
      .getFromApi<GeminiResponse[]>(
        `/Analysis/GetOldAnalysis/${keywordId}`,
        ConfigsRequest.getSkipAuthConfig()
      )
      .subscribe({
        next: (response) => {
          this.searchResults = response[0];
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

  // #region [Thêm tên miền vào từ điển]
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
      console.log('Vui lòng nhập tên miền hợp lệ.', this.newDomain);
    }
  }
  // #endregion

  // #region [Kiểm tra xem response của site có giá trị hay không]
  isResponseOfSiteHaveValue(site: string): boolean {
    return (
      this.searchResultsList.find((x) => x.siteSearch == site)?.showText == null
    );
  }
  // #endregion

  // #region [Xử lí Token Secret]
  /**
   * Gửi yêu cầu tạo mới SecretToken về phía API.
   */
  createSecretToken(service: string | null) {
    if (service == null) {
      alert('Thông số dịch vụ không xác định, vui lòng thử lại.');
      return;
    }
    this.secretTokenDTO.service = service!;
    this.apiService
      .postToApi<ResponseAPI<string>>(
        '/SecretToken/Upsert', // Đường dẫn API backend xử lý tạo/cập nhật token
        this.secretTokenDTO,
        ConfigsRequest.getSkipAuthConfig()
      )
      .subscribe({
        next: (response) => {
          if (response.success) {
            alert('Tạo mới Secret Token thành công!');
            this.addSecretToken = false;
            // Reset form hoặc load lại danh sách token nếu cần
          } else {
            alert('Tạo mới thất bại: ' + response.message);
          }
        },
        error: (err) => {
          alert(
            'Có lỗi xảy ra khi tạo Secret Token: ' +
              (err.message || 'Lỗi không xác định')
          );
          console.error('Error creating secret token:', err);
        },
      });
  }
  /**
   * Lấy danh sách SecretToken từ API khi load trang.
   */
  loadSecretTokens() {
    this.apiService
      .getFromApi<ResponseAPI<SecretTokenResponseDTO[]>>(
        '/SecretToken/GetAll',
        ConfigsRequest.getSkipAuthConfig()
      )
      .subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.listSecretDTOsMap = {};
            this.listServices.forEach((service) => {
              this.listSecretDTOsMap[service] = response.data!.filter(
                (x) => x.service === service
              );
            });
            this.listSecretDTOs = response.data;
          } else {
            this.listSecretDTOs = [];
            this.listSecretDTOsMap = {};
            console.warn(
              'Không lấy được danh sách Secret Token:',
              response.message
            );
          }
        },
        error: (err) => {
          this.listSecretDTOs = [];
          console.error('Lỗi khi lấy danh sách Secret Token:', err);
        },
      });
  }
  // #endregion

  // #region [Xác định link có thể là video content chính hay không.]
  /**
   * Xác định link có thể là video content chính hay không.
   * Trả về 'tiktok', 'youtube', 'facebook', 'google', hoặc 'other'.
   */
  getVideoPlatformType(link: string): string {
    if (!link) return 'other';
    const url = link.toLowerCase();
    if (url.includes('tiktok.com')) return 'tiktok';
    if (url.includes('youtube.com') || url.includes('youtu.be'))
      return 'youtube';
    if (url.includes('facebook.com')) return 'facebook';
    if (url.includes('google.com')) return 'google';
    return 'other';
  }
  // #endregion

  // #region [Kiểm tra link có nằm trong số dịch vụ xử lí phân tích dữ liệu cùng video]
  /**
   * Kiểm tra link có nằm trong số dịch vụ xử lí phân tích dữ liệu cùng video
   */
  isInSeviceSupport(link: string) {
    const supportServices: string[] = ['tiktok.com', 'facebook.com'];
    return supportServices.some((ss) => link.includes(ss));
  }
  // #endregion

  // #region [Phân tích video media]
  // Thêm biến cache vào class
  analysisVideoCache: Record<string, GeminiResponse> = {};

  /**
   * Phân tích link dựa vào nền tảng video, có lưu cache.
   */
  analyzeVideoLink(link: string) {
    // Nếu đã có kết quả phân tích, trả về luôn
    if (this.analysisVideoCache[link]) {
      this.mainDataAnalysisLinkSocialVideo = this.analysisVideoCache[link];
      return;
    }

    const platform = this.getVideoPlatformType(link);
    this.isLoadingDataForModal = true;
    this.isShowModal = true;

    try {
      switch (platform) {
        case 'tiktok':
          this.apiService
            .postToApi<ResponseAPI<GeminiResponse>>(
              `/VideoProcessing/ExtractContentFromLinkVideoTikTokAndAnalysis?videoUrl=${decodeURIComponent(
                link
              )}`,
              '',
              ConfigsRequest.getSkipAuthConfig()
            )
            .subscribe({
              next: (res) => {
                // Xử lý kết quả TikTok
                this.mainDataAnalysisLinkSocialVideo = res.data;
                this.mainDataAnalysisLinkSocialVideo!.showText =
                  MarkdownItConfig.formatMessageMarkToHtml(
                    res.data!.candidates[0].content.parts[0].text
                  );
                // Lưu vào cache
                this.analysisVideoCache[link] =
                  this.mainDataAnalysisLinkSocialVideo!;
                this.isLoadingDataForModal = false;
              },
              error: (err) => {
                console.error('Lỗi TikTok:', err);
                this.isLoadingDataForModal = false;
              },
            });
          break;
        case 'facebook':
          this.apiService
            .postToApi<ResponseAPI<GeminiResponse>>(
              `/VideoProcessing/ExtractContentFromLinkVideoFacebookAndAnalysis?audioUrl=${decodeURIComponent(
                link
              )}`,
              '',
              ConfigsRequest.getSkipAuthConfig()
            )
            .subscribe({
              next: (res) => {
                // Xử lý kết quả Facebook
                this.mainDataAnalysisLinkSocialVideo = res.data;
                this.mainDataAnalysisLinkSocialVideo!.showText =
                  MarkdownItConfig.formatMessageMarkToHtml(
                    res.data!.candidates[0].content.parts[0].text
                  );
                // Lưu vào cache
                this.analysisVideoCache[link] =
                  this.mainDataAnalysisLinkSocialVideo!;
                this.isLoadingDataForModal = false;
              },
              error: (err) => {
                console.error('Lỗi Facebook:', err);
                this.isLoadingDataForModal = false;
              },
            });
          break;
        case 'google':
          // Nếu là link Google, xử lý như tìm kiếm Google
          this.onAnalysisLink(link);
          this.isLoadingDataForModal = false;
          break;
        default:
          // Nếu không phải video platform, xử lý mặc định
          this.onAnalysisLink(link);
          this.isLoadingDataForModal = false;
          break;
      }
    } catch (err) {
      alert('Hiện không thể xử lí phân tích trang web của bạn');
      console.warn(err);
      this.isLoadingDataForModal = false;
    }
  }
  // #endregion
}
