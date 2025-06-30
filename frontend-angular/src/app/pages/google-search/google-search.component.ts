import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { SecretTokenPanelComponent } from '../../components/secret-token-panel/secret-token-panel.component';
import { SearchResultsComponent } from '../../components/search-results/search-results.component';
import { SearchHistoryComponent } from '../../components/search-history/search-history.component';
import { SearchFormComponent } from '../../components/search-form/search-form.component';
import { SearchService } from '../../services/search.service';
import { GoogleSearchRequest } from '../../interfaces/googleSearchService/google-search-request';

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
  searchService = inject(SearchService);

  // Properties to pass to child components
  searchParameters = this.searchService.searchParameters;
  startDate = this.searchService.startDate;
  endDate = this.searchService.endDate;
  today = this.searchService.today;
  dictionaryListSites = this.searchService.dictionaryListSites;
  listSitesSelected = this.searchService.listSitesSelected;
  newDomain = this.searchService.newDomain;
  isLoading = this.searchService.isLoading;
  errorMessageResponse = this.searchService.errorMessageResponse;
  searchResultsList = this.searchService.searchResultsList;
  selectedSite = this.searchService.selectedSite;
  searchResults = this.searchService.searchResults;
  analysisLink = this.searchService.analysisLink;
  mainDataAnalysisLinkSocialVideo =
    this.searchService.mainDataAnalysisLinkSocialVideo;
  isShowModal = this.searchService.isShowModal;
  isLoadingDataForModal = this.searchService.isLoadingDataForModal;
  showKeywordHistory = this.searchService.showKeywordHistory;
  keywordModels = this.searchService.keywordModels;
  isShowViewSettingToken = this.searchService.isShowViewSettingToken;
  listServices = this.searchService.listServices;
  listSecretDTOsMap = this.searchService.listSecretDTOsMap;
  listSelectSecretToken = this.searchService.listSelectSecretToken;
  secretTokenDTO = this.searchService.secretTokenDTO;

  ngOnInit(): void {
    this.searchService.loadSecretTokens();
    const initialTokens = this.searchService
      .listServices()
      .reduce((acc, service) => {
        acc[service] = '';
        return acc;
      }, {} as Record<string, string>);
    this.searchService.listSelectSecretToken.set(initialTokens);
  }

  // Methods to pass to child components
  onSearch() {
    this.searchService.onSearch();
  }

  onUpdateSites(event: Event) {
    this.searchService.updateSelectedSites(event);
  }

  onAddDomain() {
    this.searchService.addDomainToDictionary();
  }

  onToggleSettings() {
    this.searchService.isShowViewSettingToken.set(
      !this.searchService.isShowViewSettingToken()
    );
  }

  onCreateToken(event: string | null) {
    this.searchService.createSecretToken(event);
  }

  onSelectSite(site: string) {
    this.searchService.selectSite(site);
  }

  onAnalyzeLink(link: string) {
    this.searchService.onAnalysisLink(link);
  }

  onAnalyzeVideo(link: string) {
    this.searchService.analyzeVideoLink(link);
  }

  onToggleHistory() {
    this.searchService.toggleKeywordHistory();
    this.searchService.onTakeKeywordGoogle(this.searchParameters().q);
  }

  onLoadAnalysis(keywordId: string) {
    this.searchService.loadOldAnalysis(keywordId);
  }

  onSearchParametersChange(params: GoogleSearchRequest) {
    this.searchService.searchParameters.set(params);
  }
}
