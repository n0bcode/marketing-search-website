import { Injectable, signal } from '@angular/core';
import { GoogleSearchRequest } from '../interfaces/googleSearchService/google-search-request';
import { GeminiResponse } from '../interfaces/geminiAiService/gemini-response';
import { AnalysisLink } from '../models/analysis-link';
import { KeywordModel } from '../models/keyword-model';
import { SecretTokenResponseDTO } from '../models/dtos/secret-token-dto/secret-token-response-dto';
import { TypeServicesConstants } from '../constants/type-services-constants';
import { ApiService } from '../utils/http-client-config';
import { ResponseAPI } from '../models/response-api';
import ConfigsRequest from '../models/configs-request';
import { SearchRequest } from '../interfaces/search-request';

@Injectable({
  providedIn: 'root',
})
export class SearchService {
  // State
  searchParameters = signal<GoogleSearchRequest>({
    q: '',
    gl: 'us',
    location: '',
    hl: 'en',
    tbs: '',
    num: 10,
    type: 'search',
    engine: 'google',
    as_epq: '',
    as_oq: '',
    as_eq: '',
    as_sitesearch: '',
  });
  startDate = signal(new Date());
  endDate = signal(new Date());
  today = signal(new Date());

  isLoading = signal(false);
  isAnalyzingLink = signal(false);
  private activeRequests = 0;
  searchResults = signal<GeminiResponse | null>(null);
  searchResultsList = signal<GeminiResponse[]>([]);
  selectedSite = signal<string>('');
  analysisLink = signal<AnalysisLink | null>(null);
  errorMessageResponse = signal<string>('');
  keywordModels = signal<KeywordModel[]>([]);
  showKeywordHistory = signal(false);
  listSitesSelected = signal<string[]>([]);
  dictionaryListSites = signal<Record<string, string>>({
    'facebook.com': 'Facebook',
    'x.com': 'Twitter/X',
    'instagram.com': 'Instagram',
    'linkedin.com': 'LinkedIn',
    'tiktok.com': 'TikTok',
    'reddit.com': 'Reddit',
    'youtube.com': 'YouTube',
  });
  newDomain = signal('');

  // Secret Token Management
  addSecretToken = signal(false);
  secretTokenDTO = signal({
    name: '',
    token: '',
    service: '',
    note: '',
  });
  isShowViewSettingToken = signal(false);
  listServices = signal<string[]>(TypeServicesConstants.ActiveServices);
  listSecretDTOsMap = signal<Record<string, SecretTokenResponseDTO[]>>({});
  listSelectSecretToken = signal<Record<string, string>>({});

  mainDataAnalysisLinkSocialVideo = signal<GeminiResponse | null>(null);
  isShowModal = signal(false);
  isLoadingDataForModal = signal(false);

  constructor(private apiService: ApiService) {}

  onSearch() {
    this.isLoading.set(true);
    this.searchResultsList.set([]);
    this.errorMessageResponse.set('');

    let params = this.searchParameters();
    if (params.tbs === 'cdr:1') {
      const startDate = this.startDate();
      const endDate = this.endDate();
      params.tbs = `cdr:1,cd_min:${
        startDate.getMonth() + 1
      }/${startDate.getDate()}/${startDate.getFullYear()},cd_max:${
        endDate.getMonth() + 1
      }/${endDate.getDate()}/${endDate.getFullYear()}`;
    }

    this.selectedSite.set('');
    this.storeKeyword(params.q);

    if (params.engine === 'google') {
      if (this.listSitesSelected().length === 0) {
        this.performGoogleSearch({ ...params, as_sitesearch: '' });
      } else {
        this.listSitesSelected().forEach((site) => {
          this.performGoogleSearch({ ...params, as_sitesearch: site });
        });
      }
    } else if (params.engine === 'bing') {
      if (this.listSitesSelected().length === 0) {
        this.performBingSearch({ ...params, as_sitesearch: '' });
      } else {
        this.listSitesSelected().forEach((site) => {
          this.performBingSearch({ ...params, as_sitesearch: site });
        });
      }
    }
  }

  private performGoogleSearch(params: SearchRequest) {
    this.activeRequests++; // Tăng bộ đếm khi một yêu cầu bắt đầu

    const idTokenGoogle =
      this.listSelectSecretToken()[TypeServicesConstants.GoogleSearch];
    const idTokenGemini =
      this.listSelectSecretToken()[TypeServicesConstants.GeminiAI];

    this.apiService
      .postToApi<ResponseAPI<GeminiResponse>>(
        `/Analysis/SearchGoogleAndAnalysis?idTokenGoogleChange=${idTokenGoogle}&idTokenGeminiChange=${idTokenGemini}`,
        params,
        ConfigsRequest.getSkipAuthConfig()
      )
      .subscribe({
        next: (response) => {
          if (!response.success || !response.data) {
            this.errorMessageResponse.set(response.message);
            this.searchResults.set(null);
            return;
          }

          const data = response.data;
          data.siteSearch = params.as_sitesearch || 'default';
          data.generalSearchResultsCount = data.generalSearchResults.length;
          const cleanedText = data.candidates[0].content.parts[0].text.replace(
            /```json\n|```/g,
            ''
          );
          try {
            data.analysisData = JSON.parse(cleanedText);
          } catch (e) {
            console.error('Failed to parse analysisData as JSON:', e);
            data.analysisData = cleanedText as any; // Fallback to string if parsing fails, casting to any to bypass type error temporarily
          }

          this.searchResultsList.update((list) => [...list, data]);

          if (!this.selectedSite()) {
            this.selectedSite.set(data.siteSearch);
            this.searchResults.set(data);
          }
          console.log('performGoogleSearch API Response Data:', response.data);
        },
        error: (err) => {
          console.error(
            `Error fetching search results for site ${
              params.as_sitesearch || 'default'
            }`,
            err
          );
          this.errorMessageResponse.set(
            `Failed to load data for ${params.as_sitesearch || 'default'}: ${
              err.message
            }`
          );
        },
        complete: () => {
          this.activeRequests--; // Giảm bộ đếm khi yêu cầu hoàn thành
          if (this.activeRequests === 0) {
            this.isLoading.set(false); // Đặt isLoading thành false chỉ khi tất cả các yêu cầu đã hoàn thành
          }
        },
      });
  }

  private performBingSearch(params: SearchRequest) {
    this.activeRequests++;

    this.apiService
      .postToApi<ResponseAPI<any>>(
        `/Search/SearchBing`,
        params,
        ConfigsRequest.getSkipAuthConfig()
      )
      .subscribe({
        next: (bingResponse) => {
          if (bingResponse.success && bingResponse.data) {
            // Gọi API phân tích Bing
            const idTokenGemini =
              this.listSelectSecretToken()[TypeServicesConstants.GeminiAI];

            this.apiService
              .postToApi<ResponseAPI<GeminiResponse>>(
                `/Analysis/AnalyzeBingResults?query=${params.q}&site=${
                  params.as_sitesearch
                }&idTokenGeminiChange=${idTokenGemini || ''}`,
                bingResponse.data, // Truyền bingResponse.data vào body
                ConfigsRequest.getSkipAuthConfig()
              )
              .subscribe({
                next: (geminiResponse) => {
                  if (geminiResponse.success && geminiResponse.data) {
                    const bingResults = geminiResponse.data;
                    bingResults.siteSearch = params.as_sitesearch || 'default';
                    bingResults.generalSearchResultsCount =
                      bingResults.generalSearchResults.length;
                    const cleanedText =
                      bingResults.candidates[0].content.parts[0].text.replace(
                        /```json\n|```/g,
                        ''
                      );
                    try {
                      bingResults.analysisData = JSON.parse(cleanedText);
                    } catch (e) {
                      console.error('Failed to parse analysisData as JSON:', e);
                      bingResults.analysisData = cleanedText as any; // Fallback to string if parsing fails
                    }

                    this.searchResultsList.update((list) => [
                      ...list,
                      bingResults,
                    ]);

                    if (!this.selectedSite()) {
                      this.selectedSite.set(bingResults.siteSearch);
                      this.searchResults.set(bingResults);
                    }
                    console.log(
                      'performBingSearch Analysis API Response Data:',
                      geminiResponse.data
                    );
                  } else {
                    this.errorMessageResponse.set(geminiResponse.message);
                    this.searchResults.set(null);
                  }
                },
                error: (geminiErr) => {
                  console.error(
                    `Error analyzing Bing search results for query ${params.q}:`,
                    geminiErr
                  );
                  this.errorMessageResponse.set(
                    `Failed to analyze Bing data for ${params.q}: ${geminiErr.message}`
                  );
                },
                complete: () => {
                  this.activeRequests--;
                  if (this.activeRequests === 0) {
                    this.isLoading.set(false);
                  }
                },
              });
          } else {
            this.errorMessageResponse.set(bingResponse.message);
            this.searchResults.set(null);
            this.activeRequests--;
            if (this.activeRequests === 0) {
              this.isLoading.set(false);
            }
          }
        },
        error: (err) => {
          console.error(
            `Error fetching Bing search results for query ${params.q}:`,
            err
          );
          this.errorMessageResponse.set(
            `Failed to load Bing data for ${params.q}: ${err.message}`
          );
          this.activeRequests--;
          if (this.activeRequests === 0) {
            this.isLoading.set(false);
          }
        },
      });
  }

  onAnalysisLink(link: string) {
    this.isAnalyzingLink.set(true);
    this.errorMessageResponse.set('');
    this.analysisLink.set(null);

    const idTokenGemini =
      this.listSelectSecretToken()[TypeServicesConstants.GeminiAI];

    this.apiService
      .postToApi<AnalysisLink>(
        `/Analysis/AnalysisLink?link=${encodeURIComponent(
          link
        )}&idTokenChange=${idTokenGemini}`,
        null,
        ConfigsRequest.getSkipAuthConfig()
      )
      .subscribe({
        next: (response) => {
          this.analysisLink.set(response);
          this.isAnalyzingLink.set(false);
          console.log('onAnalysisLink API Response Data:', response);
        },
        error: (err) => {
          console.error(`Error analyzing link ${link}:`, err);
          this.errorMessageResponse.set(
            `Failed to analyze link: ${err.message}`
          );
          this.isAnalyzingLink.set(false);
        },
      });
  }

  storeKeyword(keyword: string) {
    this.apiService
      .getFromApi<ResponseAPI<KeywordModel[]>>('/Search/GetAllKeyword')
      .subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.keywordModels.set(response.data);
            console.log('storeKeyword API Response Data:', response.data);
          } else {
            console.error('Failed to load keywords:', response.message);
          }
        },
        error: (err) => {
          console.error('Error loading keywords:', err);
        },
      });
  }

  toggleKeywordHistory() {
    this.showKeywordHistory.update((value) => !value);
  }

  onTakeKeywordGoogle(keyword: string) {
    this.searchParameters.update((params) => ({
      ...params,
      q: keyword,
    }));
  }

  loadOldAnalysis(keywordId: string) {
    this.isLoading.set(true);
    this.errorMessageResponse.set('');
    this.analysisLink.set(null);

    this.apiService
      .getFromApi<ResponseAPI<AnalysisLink>>(`/Analysis/${keywordId}`)
      .subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.analysisLink.set(response.data);
            console.log('loadOldAnalysis API Response Data:', response.data);
          } else {
            this.errorMessageResponse.set(response.message);
          }
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error(
            `Error loading old analysis for keyword ID ${keywordId}:`,
            err
          );
          this.errorMessageResponse.set(
            `Failed to load old analysis: ${err.message}`
          );
          this.isLoading.set(false);
        },
      });
  }

  updateSelectedSites(event: Event) {
    const input = event.target as HTMLInputElement;
    const site = input.value;

    if (input.checked) {
      this.listSitesSelected.update((sites) => [...sites, site]);
    } else {
      this.listSitesSelected.update((sites) => sites.filter((s) => s !== site));
    }
  }

  selectSite(site: string) {
    this.selectedSite.set(site);
    const selectedResult = this.searchResultsList().find(
      (result) => result.siteSearch === site
    );
    this.searchResults.set(selectedResult || null);
  }

  addDomainToDictionary() {
    const newDomain = this.newDomain();
    if (newDomain && !this.dictionaryListSites()[newDomain]) {
      this.dictionaryListSites.update((dict) => ({
        ...dict,
        [newDomain]: newDomain,
      }));
      this.newDomain.set('');
    }
  }

  createSecretToken(service: string | null) {
    if (service) {
      this.secretTokenDTO.update((dto) => ({ ...dto, service }));
    }

    this.apiService
      .postToApi<ResponseAPI<string>>(
        '/SecretToken/Upsert',
        this.secretTokenDTO(),
        ConfigsRequest.getSkipAuthConfig()
      )
      .subscribe({
        next: (response) => {
          if (response.success) {
            console.log(
              'Secret token created/updated successfully:',
              response.data
            );
            this.loadSecretTokens();
            this.addSecretToken.set(false);
            this.secretTokenDTO.set({
              name: '',
              token: '',
              service: '',
              note: '',
            });
          } else {
            this.errorMessageResponse.set(response.message);
          }
          console.log('createSecretToken API Response Data:', response.data);
        },
        error: (err) => {
          console.error('Error creating/updating secret token:', err);
          this.errorMessageResponse.set(
            `Failed to save secret token: ${err.message}`
          );
        },
      });
  }

  loadSecretTokens() {
    this.apiService
      .getFromApi<ResponseAPI<SecretTokenResponseDTO[]>>('/SecretToken/GetAll')
      .subscribe({
        next: (response) => {
          if (response.success && response.data) {
            const groupedTokens: Record<string, SecretTokenResponseDTO[]> = {};
            const selectTokens: Record<string, string> = {};

            response.data.forEach((token) => {
              if (!groupedTokens[token.service]) {
                groupedTokens[token.service] = [];
              }
              groupedTokens[token.service].push(token);
              selectTokens[token.service] = token.id;
            });
            this.listSecretDTOsMap.set(groupedTokens);
            this.listSelectSecretToken.set(selectTokens);
            console.log('loadSecretTokens API Response Data:', response.data);
          } else {
            console.error('Failed to load secret tokens:', response.message);
          }
        },
        error: (err) => {
          console.error('Error loading secret tokens:', err);
        },
      });
  }

  analyzeVideoLink(link: string) {
    this.isLoadingDataForModal.set(true);
    this.mainDataAnalysisLinkSocialVideo.set(null);
    this.errorMessageResponse.set('');

    const idTokenGemini =
      this.listSelectSecretToken()[TypeServicesConstants.GeminiAI];

    this.apiService
      .postToApi<ResponseAPI<GeminiResponse>>(
        `/VideoProcessing/ExtractContentFromLinkVideoTikTokAndAnalysis?videoUrl=${encodeURIComponent(
          link
        )}&idTokenGeminiChange=${idTokenGemini}`,
        null,
        ConfigsRequest.getSkipAuthConfig()
      )
      .subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.mainDataAnalysisLinkSocialVideo.set(response.data);
            console.log('analyzeVideoLink API Response Data:', response.data);
          } else {
            this.errorMessageResponse.set(response.message);
          }
          this.isLoadingDataForModal.set(false);
        },
        error: (err) => {
          console.error(`Error analyzing video link ${link}:`, err);
          this.errorMessageResponse.set(
            `Failed to analyze video: ${err.message}`
          );
          this.isLoadingDataForModal.set(false);
        },
      });
  }
}
