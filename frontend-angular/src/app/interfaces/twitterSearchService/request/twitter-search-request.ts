export interface TwitterSearchRequest {
  /**
   * One query/rule/filter for matching Posts. Refer to https://t.co/rulelength to identify the max query length.
   */
  query: string;

  /**
   * The maximum number of search results to be returned by a request.
   * Default value is 10.
   */
  max_results: number;

  /**
   * This parameter is used to get the next 'page' of results.
   * The value used with the parameter is pulled directly from the response provided by the API, and should not be modified.
   */
  next_token?: string; // Optional, as it may not always be present

  /**
   * A comma separated list of Tweet fields to display.
   */
  tweet_fields: string[];

  /**
   * A comma separated list of fields to expand.
   */
  expansions: string[];

  /**
   * A comma separated list of User fields to display.
   */
  user_fields: string[];
}
