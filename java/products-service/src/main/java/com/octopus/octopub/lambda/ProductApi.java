package com.octopus.octopub.lambda;

import com.amazonaws.services.lambda.runtime.Context;
import com.amazonaws.services.lambda.runtime.RequestHandler;
import com.amazonaws.services.lambda.runtime.events.APIGatewayProxyRequestEvent;
import com.github.jasminb.jsonapi.exceptions.DocumentSerializationException;
import com.octopus.octopub.Constants;
import com.octopus.octopub.services.ProductsController;
import java.util.Base64;
import java.util.Optional;
import java.util.regex.Matcher;
import java.util.regex.Pattern;
import javax.inject.Inject;
import javax.inject.Named;
import javax.transaction.Transactional;
import lombok.NonNull;
import org.apache.commons.lang3.ObjectUtils;
import org.h2.util.StringUtils;

@Named("Products")
public class ProductApi implements RequestHandler<APIGatewayProxyRequestEvent, ProxyResponse> {

  private static final Pattern ROOT_RE = Pattern.compile("/api/products/?");
  private static final Pattern INDIVIDUAL_RE = Pattern.compile("/api/products/(?<id>\\d+)");
  private static final Pattern HEALTH_RE = Pattern.compile("/health/products/(GET|POST|\\d+/GET)");

  @Inject ProductsController productsController;

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

    return getAll(input)
        .or(() -> getOne(input))
        .or(() -> createOne(input))
        .or(() -> deleteOne(input))
        .or(() -> checkHealth(input))
        .orElse(new ProxyResponse("404", "{\"message\": \"Path not found\"}"));
  }

  /**
   * Health checks sit parallel to the /api endpoint under /health. The health endpoints mirror the
   * API, but with an additional path that indicates the http method. So, for example, a GET request
   * to /health/products/GET will return 200 OK if the service responding to /api/products is able
   * to service a GET request, and a GET request to /health/products/1/DELETE will return 200 OK if
   * the service responding to /api/products/1 is available to service a DELETE request.
   *
   * @param input The request details
   * @return The optional proxy response
   */
  private Optional<ProxyResponse> checkHealth(@NonNull final APIGatewayProxyRequestEvent input) {
    if (requestIsMatch(input, HEALTH_RE, Constants.GET_METHOD)) {
      return Optional.of(new ProxyResponse("200", "{\"message\": \"OK\"}"));
    }

    return Optional.empty();
  }

  private Optional<ProxyResponse> getAll(@NonNull final APIGatewayProxyRequestEvent input) {
    try {
      if (requestIsMatch(input, ROOT_RE, Constants.GET_METHOD)) {
        return Optional.of(
            new ProxyResponse(
                "200",
                productsController.getAll(
                    input.getMultiValueHeaders().get(Constants.ACCEPT_HEADER))));
      }
    } catch (final DocumentSerializationException e) {
      return Optional.of(new ProxyResponse("500", e.toString()));
    }

    return Optional.empty();
  }

  private Optional<ProxyResponse> getOne(@NonNull final APIGatewayProxyRequestEvent input) {
    try {

      if (requestIsMatch(input, INDIVIDUAL_RE, Constants.GET_METHOD)) {
        final Optional<String> id = getGroup(INDIVIDUAL_RE, input.getPath(), "id");

        if (id.isPresent()) {
          final String entity =
              productsController.getOne(
                  id.get(), input.getMultiValueHeaders().get(Constants.ACCEPT_HEADER));

          if (!StringUtils.isNullOrEmpty(entity)) {
            return Optional.of(new ProxyResponse("200", entity));
          }
        }
        return Optional.of(new ProxyResponse("404", "{\"message\": \"Entity not found\"}"));
      }
    } catch (final DocumentSerializationException e) {
      return Optional.of(new ProxyResponse("500", e.toString()));
    }

    return Optional.empty();
  }

  private Optional<ProxyResponse> deleteOne(@NonNull final APIGatewayProxyRequestEvent input) {
    try {

      if (requestIsMatch(input, INDIVIDUAL_RE, Constants.DELETE_METHOD)) {
        final Optional<String> id = getGroup(INDIVIDUAL_RE, input.getPath(), "id");

        if (id.isPresent()) {
          final boolean result =
              productsController.delete(
                  id.get(), input.getMultiValueHeaders().get(Constants.ACCEPT_HEADER));

          if (result) {
            return Optional.of(new ProxyResponse("204"));
          }
        }
        return Optional.of(new ProxyResponse("404"));
      }
    } catch (final DocumentSerializationException e) {
      return Optional.of(new ProxyResponse("500", e.toString()));
    }

    return Optional.empty();
  }

  private Optional<ProxyResponse> createOne(@NonNull final APIGatewayProxyRequestEvent input) {
    try {
      if (requestIsMatch(input, ROOT_RE, Constants.POST_METHOD)) {
        return Optional.of(
            new ProxyResponse(
                "200",
                productsController.create(
                    getBody(input),
                    input.getMultiValueHeaders().get(Constants.ACCEPT_HEADER))));
      }
    } catch (final DocumentSerializationException e) {
      return Optional.of(
          new ProxyResponse(
              "500",
              "{\"message\": \"" + e + "\", \"body\": \"" + getBody(input) + "\"}"));
    } catch (final RuntimeException ex) {
      System.out.println(ex);
      throw ex;
    }

    return Optional.empty();
  }

  private Optional<String> getGroup(
      @NonNull final Pattern pattern, final Object input, @NonNull final String group) {
    if (input == null) return Optional.empty();

    final Matcher matcher = pattern.matcher(input.toString());

    if (matcher.find()) {
      return Optional.of(matcher.group(group));
    }

    return Optional.empty();
  }

  private boolean requestIsMatch(
      @NonNull final APIGatewayProxyRequestEvent input,
      @NonNull final Pattern regex,
      @NonNull final String method) {
    final String path = ObjectUtils.defaultIfNull(input.getPath(), "");
    final String requestMethod =
        ObjectUtils.defaultIfNull(input.getHttpMethod(), "").toLowerCase();
    return regex.matcher(path).matches() && method.toLowerCase().equals(requestMethod);
  }

  private String getBody(@NonNull final APIGatewayProxyRequestEvent input) {
    final String body = ObjectUtils.defaultIfNull(input.getBody(), "");
    final String isBase64Encoded =
        ObjectUtils.defaultIfNull(input.getIsBase64Encoded(), "")
            .toString()
            .toLowerCase();

    if ("true".equals(isBase64Encoded)) {
      return new String(Base64.getDecoder().decode(body));
    }

    return body;
  }
}
