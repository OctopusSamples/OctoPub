package com.octopus.octopub.lambda;

import com.amazonaws.services.lambda.runtime.Context;
import com.amazonaws.services.lambda.runtime.RequestHandler;
import com.github.jasminb.jsonapi.exceptions.DocumentSerializationException;
import com.octopus.octopub.Constants;
import com.octopus.octopub.services.LambdaUtils;
import com.octopus.octopub.services.ProductsController;
import java.util.Arrays;
import java.util.Map;
import java.util.Optional;
import java.util.regex.Matcher;
import java.util.regex.Pattern;
import javax.inject.Inject;
import javax.inject.Named;
import javax.transaction.Transactional;
import lombok.NonNull;

@Named("Products")
public class ProductApi implements RequestHandler<Map<String, Object>, ProxyResponse> {

  private static final Pattern ROOT_RE = Pattern.compile("^/api/products/?$");
  private static final Pattern INDIVIDUAL_RE = Pattern.compile("^/api/products/(?<id>\\d+)$");
  private static final Pattern[] HEALTH_RE = {
    Pattern.compile("^/health/products/GET$"), Pattern.compile("^/health/products/(?<id>\\d+)/GET$")
  };

  @Inject ProductsController productsController;

  @Inject LambdaUtils lambdaUtils;

  /**
   * See https://github.com/quarkusio/quarkus/issues/5811 for why we need @Transactional.
   *
   * @param stringObjectMap The request details
   * @param context The request context
   * @return The proxy response
   */
  @Override
  @Transactional
  public ProxyResponse handleRequest(
      @NonNull final Map<String, Object> stringObjectMap, @NonNull final Context context) {

    return getAll(stringObjectMap)
        .or(() -> getOne(stringObjectMap))
        .or(() -> checkHealth(stringObjectMap))
        .orElse(new ProxyResponse("404", "\"message\": \"Path not found\""));
  }

  /**
   * Health checks sit parallel to the /api endpoint under /health. The health endpoints mirror the
   * API, but with an additional path that indicates the http method. So, for example, a GET request
   * to /health/products/GET will return 200 OK if the service responding to /api/products is able
   * to service a GET request, and a GET request to /health/products/1/DELETE will return 200 OK if
   * the service responding to /api/products/1 is available to service a DELETE request.
   *
   * @param stringObjectMap The request details
   * @return The optional proxy response
   */
  private Optional<ProxyResponse> checkHealth(@NonNull final Map<String, Object> stringObjectMap) {

    final String path = stringObjectMap.get("path").toString();

    if (Arrays.stream(HEALTH_RE).anyMatch(m -> m.matcher(path).matches())) {
      return Optional.of(new ProxyResponse("200", "{\"message\": \"OK\"}"));
    }

    return Optional.empty();
  }

  private Optional<ProxyResponse> getAll(@NonNull final Map<String, Object> stringObjectMap) {
    try {
      final String path = stringObjectMap.get("path").toString();

      if (ROOT_RE.matcher(path).matches()) {
        return Optional.of(
            new ProxyResponse(
                "200",
                productsController.getAll(
                    lambdaUtils.getHeader(stringObjectMap, Constants.ACCEPT_HEADER))));
      }
    } catch (final DocumentSerializationException e) {
      return Optional.of(new ProxyResponse("500", e.toString()));
    }

    return Optional.empty();
  }

  private Optional<ProxyResponse> getOne(@NonNull final Map<String, Object> stringObjectMap) {
    try {
      final String path = stringObjectMap.get("path").toString();

      final Matcher matcher = INDIVIDUAL_RE.matcher(path);
      if (matcher.matches()) {
        return Optional.of(
            new ProxyResponse(
                "200",
                productsController.getOne(
                    matcher.group("id"),
                    lambdaUtils.getHeader(stringObjectMap, Constants.ACCEPT_HEADER))));
      }
    } catch (final DocumentSerializationException e) {
      return Optional.of(new ProxyResponse("500", e.toString()));
    }

    return Optional.empty();
  }
}
