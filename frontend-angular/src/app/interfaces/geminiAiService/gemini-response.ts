export interface GeminiResponse {
  showText: string;
  candidates: Candidate[];
  usageMetadata: UsageMetadata;
  modelVersion: string;
  generalSearchResults: GeneralSearchResult[];
  generalSearchResultsCount: number;
  siteSearch: string;
}

// #region [MAIN INTERFACE]
export interface Candidate {
  content: ContentResponse;
  finishReason: string;
  citationMetadata: CitationMetadata;
  avgLogprobs?: number; // Có thể là null hoặc undefined
}

export interface ContentResponse {
  parts: PartResponse[];
  role: string;
}

export interface PartResponse {
  text: string;
}

export interface CitationMetadata {
  citationSources: CitationSource[];
}

export interface CitationSource {
  startIndex: number;
  endIndex: number;
}

export interface UsageMetadata {
  promptTokenCount: number;
  candidatesTokenCount: number;
  totalTokenCount: number;
  promptTokensDetails: TokensDetails[];
  candidatesTokensDetails: TokensDetails[];
}

export interface TokensDetails {
  modality: string;
  tokenCount: number;
}
// #endregion

export interface GeneralSearchResult {
  id: string;
  title: string;
  description: string;
  descriptionsource: string;
  url: string;
  createdAt: Date;
  author: string;
  source: string;
}
