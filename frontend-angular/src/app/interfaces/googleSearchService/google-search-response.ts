import { GoogleSearchRequest } from './google-search-request.js';
export interface GoogleSearchResponse {
    searchParameters: SearchRequest;
    knowledgeGraph: KnowledgeGraph;
    organic: OrganicSearchResult[];
    relatedSearches: RelatedSearch[];
    credits: number;
}

export interface KnowledgeGraph {
    title: string;
    type: string;
    imageUrl: string;
    description: string;
    descriptionSource: string;
    descriptionLink: string;
    attributes: { [key: string]: string };
}

export interface OrganicSearchResult {
    title: string;
    link: string;
    snippet: string;
    position: number;
    date?: string; // Có thể để null
}

export interface RelatedSearch {
    query: string;
}