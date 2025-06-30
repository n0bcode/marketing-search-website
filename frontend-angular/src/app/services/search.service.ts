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
import { MarkdownItConfig } from '../utils/markdown-it-config';

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

    if (this.listSitesSelected().length === 0) {
      this.performSearch({ ...params, as_sitesearch: '' });
    } else {
      this.listSitesSelected().forEach((site) => {
        this.performSearch({ ...params, as_sitesearch: site });
      });
    }
  }

  private performSearch(params: GoogleSearchRequest) {
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
          this.isLoading.set(false);
          console.log('performSearch API Response Data:', response.data);
        },
        error: (err) => {
          console.error(
            `Error fetching search results for site ${
              params.as_sitesearch || 'default'
            }:`,
            err
          );
          this.errorMessageResponse.set(
            `Failed to load data for ${params.as_sitesearch || 'default'}: ${
              err.message
            }`
          );
          this.isLoading.set(
            this.searchResultsList().length < this.listSitesSelected().length
          );
        },
      });
  }

  onAnalysisLink(link: string) {
    this.isLoading.set(true);
    this.errorMessageResponse.set('');
    this.analysisLink.set(null);

    const idTokenGemini =
      this.listSelectSecretToken()[TypeServicesConstants.GeminiAI];

    this.apiService
      .postToApi<ResponseAPI<AnalysisLink>>(
        `/Analysis/AnalysisLink?link=${encodeURIComponent(
          link
        )}&idTokenChange=${idTokenGemini}`,
        null,
        ConfigsRequest.getSkipAuthConfig()
      )
      .subscribe({
        next: (response) => {
          if (!response.success || !response.data) {
            this.errorMessageResponse.set(response.message);
            return;
          }
          this.analysisLink.set(response.data);
          this.isLoading.set(false);
          console.log('onAnalysisLink API Response Data:', response.data);
        },
        error: (err) => {
          console.error(`Error analyzing link ${link}:`, err);
          this.errorMessageResponse.set(
            `Failed to analyze link: ${err.message}`
          );
          this.isLoading.set(false);
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
        `/VideoProcessing/AnalyzeVideo?videoUrl=${encodeURIComponent(
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
