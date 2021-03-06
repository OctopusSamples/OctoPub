package com.octopus.octopub.application.lambda;

import com.amazonaws.services.lambda.runtime.Context;
import com.amazonaws.services.lambda.runtime.RequestHandler;
import com.amazonaws.services.lambda.runtime.events.APIGatewayProxyRequestEvent;
import com.octopus.octopub.application.Constants;
import com.octopus.octopub.domain.exceptions.EntityNotFound;
import com.octopus.octopub.domain.exceptions.InvalidInput;
import com.octopus.octopub.domain.exceptions.Unauthorized;
import com.octopus.octopub.domain.handlers.HealthHandler;
import com.octopus.octopub.domain.handlers.ProductsHandler;
import cz.jirutka.rsql.parser.RSQLParserException;
import java.util.ArrayList;
import java.util.Base64;
import java.util.List;
import java.util.Map;
import java.util.Optional;
import java.util.regex.Matcher;
import java.util.regex.Pattern;
import java.util.stream.Collectors;
import javax.inject.Inject;
import javax.inject.Named;
import javax.transaction.Transactional;
import lombok.NonNull;
import org.apache.commons.lang3.ObjectUtils;
import org.apache.commons.text.StringEscapeUtils;

/** The Lambda entry point used to return product resources. */
@Named("Products")
public class ProductApi implements RequestHandler<APIGatewayProxyRequestEvent, ProxyResponse> {

  /** A regular expression matching the collection of entities. */
  private static final Pattern ROOT_RE = Pattern.compile("/api/products/?");
  /** A regular expression matching a single entity. */
  private static final Pattern INDIVIDUAL_RE = Pattern.compile("/api/products/(?<id>\\d+)");
  /** A regular expression matching a health endpoint. */
  private static final Pattern HEALTH_RE =
      Pattern.compile("/health/products/(GET|POST|[A-Za-z0-9]+/(GET|DELETE|PATCH))");

  @Inject ProductsHandler productsHandler;

  @Inject HealthHandler healthHandler;

  /**
   * See https://github.com/quarkusio/quarkus/issues/5811 for why we need @Transactional.
   *
   * @param input The request details
   * @param context The request context
   * @return The proxy response
   */
  @Override
  @Transactional
  public ProxyResponse handleRequest(
      @NonNull final APIGatewayProxyRequestEvent input, @NonNull final Context context) {

    /*
     Lambdas don't enjoy the same middleware and framework support as web servers, so we are
     on our own with functionality such as routing requests to handlers. This code simply calls
     each handler to find the first one that responds to the request.
    */
    return getAll(input)
        .or(() -> getOne(input))
        .or(() -> createOne(input))
        .or(() -> deleteOne(input))
        .or(() -> updateOne(input))
        .or(() -> checkHealth(input))
        .orElse(new ProxyResponse("404", "{\"errors\": [{\"title\": \"Path not found\"}]}"));
  }

  /**
   * Health checks sit parallel to the /api endpoint under /health. The health endpoints mirror the
   * API, but with an additional path that indicates the http method. So, for example, a GET request
   * to /health/products/GET will return 200 OK if the service responding to /api/products is able
   * to service a GET request, and a GET request to /health/products/1/DELETE will return 200 OK if
   * the service responding to /api/products/1 is available to service a DELETE request.
   *
   * <p>This approach was taken to support the fact that Lambdas may well have unique services
   * responding to each individual endpoint. For example, you may have a dedicated lambda fetching
   * resource collections (i.e. /api/products), and a dedicated lambda fetching individual resources
   * (i.e. /api/products/1). The health of these lambdas may be independent of one another.
   *
   * <p>This is unlike a traditional web service, where it is usually taken for granted that a
   * single application responds to all these requests, and therefore a single health endpoint can
   * represent the status of all endpoints.
   *
   * <p>By ensuring every path has a matching health endpoint, we allow clients to verify the status
   * of the service without having to know which lambdas respond to which requests. This does mean
   * that a client may need to verify the health of half a dozen endpoints to fully determine the
   * state of the client's dependencies, but this is a more accurate representation of the health of
   * the system.
   *
   * <p>This particular service will typically be deployed with one lambda responding to many
   * endpoints, but clients can not assume this is always the case, and must check the health of
   * each endpoint to accurately evaluate the health of the service.
   *
   * @param input The request details
   * @return The optional proxy response
   */
  private Optional<ProxyResponse> checkHealth(@NonNull final APIGatewayProxyRequestEvent input) {

    if (requestIsMatch(input, HEALTH_RE, Constants.GET_METHOD)) {
      try {
        return Optional.of(
            new ProxyResponse(
                "200",
                healthHandler.getHealth(
                    input.getPath().substring(0, input.getPath().lastIndexOf("/")),
                    input.getPath().substring(input.getPath().lastIndexOf("/")))));
      } catch (final Exception e) {
        e.printStackTrace();
        return Optional.of(buildError(e));
      }
    }

    return Optional.empty();
  }

  /**
   * Update a collection of products.
   *
   * @param input The Lambda request.
   * @return The Lambda response.
   */
  private Optional<ProxyResponse> getAll(@NonNull final APIGatewayProxyRequestEvent input) {
    try {
      if (requestIsMatch(input, ROOT_RE, Constants.GET_METHOD)) {
        return Optional.of(
            new ProxyResponse(
                "200",
                productsHandler.getAll(
                    getAllHeaders(
                        input.getMultiValueHeaders(), input.getHeaders(), Constants.ACCEPT_HEADER),
                    getAllQueryParams(
                            input.getMultiValueQueryStringParameters(),
                            input.getQueryStringParameters(),
                            Constants.FILTER_QUERY_PARAM)
                        .stream()
                        .findFirst()
                        .orElse(null))));
      }
    } catch (final Unauthorized e) {
      return Optional.of(buildUnauthorizedRequest(e));
    } catch (final RSQLParserException e) {
      return Optional.of(buildBadRequest(e));
    } catch (final Exception e) {
      e.printStackTrace();
      return Optional.of(buildError(e));
    }

    return Optional.empty();
  }

  /**
   * Return a product.
   *
   * @param input The Lambda request.
   * @return The Lambda response.
   */
  private Optional<ProxyResponse> getOne(@NonNull final APIGatewayProxyRequestEvent input) {
    try {

      if (requestIsMatch(input, INDIVIDUAL_RE, Constants.GET_METHOD)) {
        final Optional<String> id = getGroup(INDIVIDUAL_RE, input.getPath(), "id");

        if (id.isPresent()) {
          final String entity =
              productsHandler.getOne(
                  id.get(),
                  getAllHeaders(
                      input.getMultiValueHeaders(), input.getHeaders(), Constants.ACCEPT_HEADER));

          return Optional.of(new ProxyResponse("200", entity));
        }
        return Optional.of(buildNotFound());
      }
    } catch (final EntityNotFound ex) {
      return Optional.of(buildNotFound());
    } catch (final Exception e) {
      e.printStackTrace();
      return Optional.of(buildError(e));
    }

    return Optional.empty();
  }

  /**
   * Delete a product.
   *
   * @param input The Lambda request.
   * @return The Lambda response.
   */
  private Optional<ProxyResponse> deleteOne(@NonNull final APIGatewayProxyRequestEvent input) {
    try {

      if (requestIsMatch(input, INDIVIDUAL_RE, Constants.DELETE_METHOD)) {
        final Optional<String> id = getGroup(INDIVIDUAL_RE, input.getPath(), "id");

        if (id.isPresent()) {
          final boolean result =
              productsHandler.delete(
                  id.get(),
                  getAllHeaders(
                      input.getMultiValueHeaders(), input.getHeaders(), Constants.ACCEPT_HEADER));

          if (result) {
            return Optional.of(new ProxyResponse("204"));
          }
        }
        return Optional.of(buildNotFound());
      }
    } catch (final EntityNotFound ex) {
      return Optional.of(buildNotFound());
    } catch (final Exception e) {
      e.printStackTrace();
      return Optional.of(buildError(e));
    }

    return Optional.empty();
  }

  /**
   * Create a product.
   *
   * @param input The Lambda request.
   * @return The Lambda response.
   */
  private Optional<ProxyResponse> createOne(@NonNull final APIGatewayProxyRequestEvent input) {
    try {
      if (requestIsMatch(input, ROOT_RE, Constants.POST_METHOD)) {
        return Optional.of(
            new ProxyResponse(
                "200",
                productsHandler.create(
                    getBody(input),
                    getAllHeaders(
                        input.getMultiValueHeaders(), input.getHeaders(), Constants.ACCEPT_HEADER),
                    getAllHeaders(
                            input.getMultiValueHeaders(),
                            input.getHeaders(),
                            Constants.AUTHORIZATION_HEADER)
                        .stream()
                        .findFirst()
                        .orElse(null))));
      }
    } catch (final Unauthorized e) {
      return Optional.of(buildUnauthorizedRequest(e));
    } catch (final InvalidInput e) {
      return Optional.of(buildBadRequest(e));
    } catch (final Exception e) {
      e.printStackTrace();
      return Optional.of(buildError(e, getBody(input)));
    }

    return Optional.empty();
  }

  /**
   * Update a product.
   *
   * @param input The Lambda request.
   * @return The Lambda response.
   */
  private Optional<ProxyResponse> updateOne(@NonNull final APIGatewayProxyRequestEvent input) {
    try {
      if (requestIsMatch(input, INDIVIDUAL_RE, Constants.PATCH_METHOD)) {
        // attempt to extract the ID from the path
        final Optional<String> id = getGroup(INDIVIDUAL_RE, input.getPath(), "id");
        // This should be present, but be defensive and check
        if (id.isPresent()) {
          return Optional.of(
              new ProxyResponse(
                  "200",
                  productsHandler.update(
                      id.get(),
                      getBody(input),
                      getAllHeaders(
                          input.getMultiValueHeaders(),
                          input.getHeaders(),
                          Constants.ACCEPT_HEADER))));
        }
        // If the id was not found in the path, return a 404
        return Optional.of(buildNotFound());
      }
    } catch (final InvalidInput e) {
      return Optional.of(buildBadRequest(e));
    } catch (final EntityNotFound ex) {
      // If the resource didn't exist in the system to be updated, return a 404
      return Optional.of(buildNotFound());
    } catch (final Exception e) {
      // assume all other exceptions are a server side issue, and return a 500
      e.printStackTrace();
      return Optional.of(buildError(e, getBody(input)));
    }

    // This handler does not recognize the request, so return an empty result
    return Optional.empty();
  }

  /**
   * Get the regex group from the pattern for the input.
   *
   * @param pattern The regex pattern.
   * @param input The input to apply the pattern to.
   * @param group The group name to return.
   * @return The regex group value.
   */
  private Optional<String> getGroup(
      @NonNull final Pattern pattern, final Object input, @NonNull final String group) {
    if (input == null) {
      return Optional.empty();
    }

    final Matcher matcher = pattern.matcher(input.toString());

    if (matcher.find()) {
      return Optional.of(matcher.group(group));
    }

    return Optional.empty();
  }

  /**
   * Gets headers from every collection they might be in.
   *
   * @param multiHeaders The map containing headers with multiple values.
   * @param headers The map containing headers with one value.
   * @param header The name of the header.
   * @return The list of header values.
   */
  private List<String> getAllHeaders(
      final Map<String, List<String>> multiHeaders,
      final Map<String, String> headers,
      @NonNull final String header) {
    final List<String> values = new ArrayList<String>(getMultiHeaders(multiHeaders, header));
    values.addAll(getHeaders(headers, header));
    return values;
  }

  /**
   * Headers are case insensitive, but the maps we get from Lambda are case sensitive, so we need to
   * have some additional logic to get the available headers.
   *
   * @param headers The list of headers
   * @param header The name of the header to return
   * @return The list of header values
   */
  private List<String> getMultiHeaders(
      final Map<String, List<String>> headers, @NonNull final String header) {
    if (headers == null) {
      return List.of();
    }

    return headers.entrySet().stream()
        .filter(e -> header.equalsIgnoreCase(e.getKey()))
        .flatMap(e -> e.getValue().stream())
        .collect(Collectors.toList());
  }

  /**
   * Headers are case insensitive, but the maps we get from Lambda are case sensitive, so we need to
   * have some additional logic to get the available headers.
   *
   * @param headers The list of headers
   * @param header The name of the header to return
   * @return The list of header values
   */
  private List<String> getHeaders(final Map<String, String> headers, @NonNull final String header) {
    if (headers == null) {
      return List.of();
    }

    return headers.entrySet().stream()
        .filter(e -> header.equalsIgnoreCase(e.getKey()))
        .map(e -> e.getValue())
        .collect(Collectors.toList());
  }

  /**
   * Gets headers from every collection they might be in.
   *
   * @param multiQuery The map containing paarms with multiple values.
   * @param query The map containing query params with one value.
   * @param header The name of the header.
   * @return The list of header values.
   */
  private List<String> getAllQueryParams(
      final Map<String, List<String>> multiQuery,
      final Map<String, String> query,
      @NonNull final String header) {
    final List<String> values = new ArrayList<String>(getMultiQuery(multiQuery, header));
    values.addAll(getQuery(query, header));
    return values;
  }

  /**
   * Headers are case insensitive, but the maps we get from Lambda are case sensitive, so we need to
   * have some additional logic to get the available headers.
   *
   * @param query The list of query params
   * @param header The name of the query param to return
   * @return The list of query params
   */
  private List<String> getMultiQuery(
      final Map<String, List<String>> query, @NonNull final String header) {
    if (query == null) {
      return List.of();
    }

    return query.entrySet().stream()
        .filter(e -> header.equalsIgnoreCase(e.getKey()))
        .flatMap(e -> e.getValue().stream())
        .collect(Collectors.toList());
  }

  /**
   * Headers are case insensitive, but the maps we get from Lambda are case sensitive, so we need to
   * have some additional logic to get the available headers.
   *
   * @param query The list of query params
   * @param header The name of the header to return
   * @return The list of header values
   */
  private List<String> getQuery(final Map<String, String> query, @NonNull final String header) {
    if (query == null) {
      return List.of();
    }

    return query.entrySet().stream()
        .filter(e -> header.equalsIgnoreCase(e.getKey()))
        .map(e -> e.getValue())
        .collect(Collectors.toList());
  }

  /**
   * Determine if the Lambda request matches path and method.
   *
   * @param input The Lmabda request.
   * @param regex The path regex.
   * @param method The HTTP method.
   * @return true if this request matches the supplied values, and false otherwise.
   */
  private boolean requestIsMatch(
      @NonNull final APIGatewayProxyRequestEvent input,
      @NonNull final Pattern regex,
      @NonNull final String method) {
    final String path = ObjectUtils.defaultIfNull(input.getPath(), "");
    final String requestMethod = ObjectUtils.defaultIfNull(input.getHttpMethod(), "").toLowerCase();
    return regex.matcher(path).matches() && method.toLowerCase().equals(requestMethod);
  }

  /**
   * Get the request body, and deal with the fact that it may be base64 encoded.
   *
   * @param input The Lambda request
   * @return The unencoded request body
   */
  private String getBody(@NonNull final APIGatewayProxyRequestEvent input) {
    final String body = ObjectUtils.defaultIfNull(input.getBody(), "");
    final String isBase64Encoded =
        ObjectUtils.defaultIfNull(input.getIsBase64Encoded(), "").toString().toLowerCase();

    if ("true".equals(isBase64Encoded)) {
      return new String(Base64.getDecoder().decode(body));
    }

    return body;
  }

  /**
   * Build an error object including the exception name and the body of the request that was sent.
   * https://jsonapi.org/format/#error-objects
   *
   * @param ex The exception
   * @param requestBody The request body
   * @return The ProxyResponse representing the error.
   */
  private ProxyResponse buildError(@NonNull final Exception ex, final String requestBody) {
    return new ProxyResponse(
        "500",
        "{\"errors\": [{\"code\": \""
            + ex.getClass().getCanonicalName()
            + "\", \"meta\": {\"requestBody\": \""
            + StringEscapeUtils.escapeJson(requestBody)
            + "\"}}]}");
  }

  /**
   * Build an error object including the exception name. https://jsonapi.org/format/#error-objects
   *
   * @param ex The exception
   * @return The ProxyResponse representing the error.
   */
  private ProxyResponse buildError(@NonNull final Exception ex) {
    return new ProxyResponse(
        "500", "{\"errors\": [{\"code\": \"" + ex.getClass().getCanonicalName() + "\"}]}");
  }

  /**
   * Build a error object for a 404 not found error. https://jsonapi.org/format/#error-objects
   *
   * @return The ProxyResponse representing the error.
   */
  private ProxyResponse buildNotFound() {
    return new ProxyResponse("404", "{\"errors\": [{\"title\": \"Resource not found\"}]}");
  }

  /**
   * Build an error object including the exception name. https://jsonapi.org/format/#error-objects
   *
   * @param ex The exception
   * @return The ProxyResponse representing the error.
   */
  private ProxyResponse buildBadRequest(@NonNull final Exception ex) {
    return new ProxyResponse(
        "400", "{\"errors\": [{\"code\": \"" + ex.getClass().getCanonicalName() + "\"}]}");
  }

  /**
   * Build an error object including the exception name. https://jsonapi.org/format/#error-objects
   *
   * @param ex The exception
   * @return The ProxyResponse representing the error.
   */
  private ProxyResponse buildUnauthorizedRequest(@NonNull final Exception ex) {
    return new ProxyResponse("403", "{\"errors\": [{\"title\": \"Unauthorized\"}]}");
  }
}
