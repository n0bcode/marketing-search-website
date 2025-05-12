import { Component, signal } from '@angular/core';
import { ApiService } from '../../utils/http-client-config';
import { TwitterSearchTweetRequest } from '../../interfaces/twitterSearchService/request/twitter-search-tweet-request';
import { GeminiResponse } from '../../interfaces/geminiAiService/gemini-response';
import { MarkdownItConfig } from '../../utils/markdown-it-config';
import ConfigsRequest from '../../models/configs-request';
import { AnalysisLink } from '../../models/analysis-link';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-twitter-search',
  imports: [CommonModule, MatInputModule, FormsModule],
  templateUrl: './twitter-search.component.html',
  styleUrls: ['./twitter-search.component.css'],
})
export class TwitterSearchComponent {
  isLoading = signal(false);
  searchQuery: string = '';
  searchNum: number = 10;
  twitterSearchResults: GeminiResponse | null = null;
  analysisLink: AnalysisLink | null = null;
  analysisLinks: AnalysisLink[] = [];

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

  constructor(private apiService: ApiService) {}

  // Phương thức tìm kiếm Twitter
  onSearchTwitter() {
    this.twitterSearchParameters.query = this.searchQuery;
    this.twitterSearchParameters.max_results = this.searchNum;
    this.isLoading.set(true);

    this.apiService
      .postToApi<GeminiResponse>(
        '/Analysis/SearchTwitterAndAnalysis',
        this.twitterSearchParameters,
        ConfigsRequest.getSkipAuthConfig()
      )
      .subscribe({
        next: (response) => {
          this.twitterSearchResults = response;
          this.twitterSearchResults.showText =
            MarkdownItConfig.formatMessageMarkToHtml(
              response.candidates[0].content.parts[0].text
            );
          this.isLoading.set(false);
          console.log(this.twitterSearchResults);
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
}
