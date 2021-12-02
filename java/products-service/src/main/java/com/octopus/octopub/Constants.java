package com.octopus.octopub;

/**
 * Constants used by the service.
 */
public final class Constants {
  public static final String CREATED_ACTION = "CREATED";
  public static final String DELETED_ACTION = "DELETED";
  public static final String UPDATED_ACTION = "UPDATED";
  public static final String UPDATED_FAILED_PARTITION_MISMATCH_ACTION =
      "UPDATED_FAILED_PARTITION_MISMATCH";
  public static final String JSONAPI_CONTENT_TYPE = "application/vnd.api+json";
  public static final String JSON_CONTENT_TYPE = "application/json";
  public static final String DEFAULT_PARTITION = "main";
  public static final String ACCEPT_HEADER = "Accept";
  public static final String FILTER_QUERY_PARAM = "filter";
  public static final String API_GATEWAY_API_KEY_HEADER = "X-API-Key";
  public static final String MICROSERVICE_NAME = "ProductsService";
  public static final String ACCEPT_VERSION_INFO = "products-version";
  public static final String ACCEPT_PARTITION_INFO = "dataPartition";
  public static final String GET_METHOD = "get";
  public static final String POST_METHOD = "post";
  public static final String PATCH_METHOD = "patch";
  public static final String DELETE_METHOD = "delete";
}
