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
          data.showText = data.candidates[0].content.parts[0].text.replace(
            /```html|```/g,
            ''
          );
          data.siteSearch = params.as_sitesearch || 'default';
          data.generalSearchResultsCount = data.generalSearchResults.length;

          this.searchResultsList.update((list) => [...list, data]);

          if (!this.selectedSite()) {
            this.selectedSite.set(data.siteSearch);
            this.searchResults.set(data);
          }
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error(
            `Error fetching search results for site ${
              params.as_sitesearch || 'default'
            }:`,
            err
          );
          this.isLoading.set(
            this.searchResultsList().length < this.listSitesSelected().length
          );
        },
      });
  }

  onAnalysisLink(link: string) {
    // Implement analysis link logic here
  }

  storeKeyword(keyword: string) {
    // Implement store keyword logic here
  }

  toggleKeywordHistory() {
    this.showKeywordHistory.update((value) => !value);
  }

  onTakeKeywordGoogle() {
    // Implement take keyword logic here
  }

  loadOldAnalysis(keywordId: string) {
    // Implement load old analysis logic here
  }

  updateSelectedSites(event: Event) {
    const input = event.target as HTMLInputElement;
    const site = input.value;

    if (input.checked) {
      this.listSitesSelected.update((sites) => [...sites, site]);
    } else {
      this.listSitesSelected.update((sites) =>
        sites.filter((s) => s !== site)
      );
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
    // Implement create secret token logic here
  }

  loadSecretTokens() {
    // Implement load secret tokens logic here
  }

  analyzeVideoLink(link: string) {
    // Implement analyze video link logic here
  }
}
