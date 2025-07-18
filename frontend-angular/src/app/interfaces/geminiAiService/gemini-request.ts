export interface GeminiRequest {
  contents: ContentRequest[];
}

export interface ContentRequest {
  parts: PartRequest[];
}

export interface PartRequest {
  text: string;
}

