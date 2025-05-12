import { TwitterSearchRequest } from './twitter-search-request';

export interface TwitterSearchTweetRequest extends TwitterSearchRequest {
  /**
   * YYYY-MM-DDTHH:mm:ssZ. The oldest UTC timestamp from which the Posts will be provided.
   * Timestamp is in second granularity and is inclusive.
   */
  start_time: string;

  /**
   * YYYY-MM-DDTHH:mm:ssZ. The newest, most recent UTC timestamp to which the Posts will be provided.
   * Timestamp is in second granularity and is exclusive.
   */
  end_time: string;

  /**
   * Returns results with a Post ID greater than (that is, more recent than) the specified ID.
   */
  since_id: string;

  /**
   * Returns results with a Post ID less than (that is, older than) the specified ID.
   */
  until_id: string;

  /**
   * This parameter is used to get the next 'page' of results.
   * The value used with the parameter is pulled directly from the response provided by the API, and should not be modified.
   */
  pagination_token?: string; // Optional, as it may not always be present

  /**
   * This order in which to return results.
   */
  sort_order: string; // Using string as it is an enum in C#

  /**
   * A comma separated list of Media fields to display.
   */
  media_fields: string[];

  /**
   * A comma separated list of Poll fields to display.
   */
  poll_fields: string[];

  /**
   * A comma separated list of Place fields to display.
   */
  place_fields: string[];
}
