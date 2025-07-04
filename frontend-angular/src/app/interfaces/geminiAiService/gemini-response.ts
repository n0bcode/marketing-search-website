export interface GeminiResponse {
  showText: string;
  candidates: Candidate[];
  usageMetadata: UsageMetadata | undefined;
  modelVersion: string | undefined;
  generalSearchResults: GeneralSearchResult[];
  generalSearchResultsCount: number;
  siteSearch: string;
  note: string | undefined;
  analysisData?: AnalysisResult; // Add this to represent the structured AI analysis
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
  date: string;
  createdAt: Date;
  author: string;
  source: string;
}

// #region [STRUCTURED AI ANALYSIS INTERFACES]

export interface AnalysisResult {
  tieuDe: string;
  nguonDuLieu: string;
  danhGia: Evaluation;
  thongBaoKhongCoDuLieu: string;
}

export interface Evaluation {
  tichCuc: ArticleEvaluation;
  tieuCuc: ArticleEvaluation;
  danhGiaChung: string;
}

export interface ArticleEvaluation {
  soLuong: number;
  baiViet: Article[];
}

export interface Article {
  tomTat: string;
  lyDo: string;
  lienKet: string;
}

// #endregion
