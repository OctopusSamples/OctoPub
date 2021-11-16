package com.octopus.octopub.lambda;

import com.amazonaws.services.lambda.runtime.Context;
import com.amazonaws.services.lambda.runtime.RequestHandler;
import com.github.jasminb.jsonapi.exceptions.DocumentSerializationException;
import com.octopus.octopub.Constants;
import com.octopus.octopub.services.LambdaUtils;
import com.octopus.octopub.services.ProductsController;
import java.util.Base64;
import java.util.Map;
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
public class ProductApi implements RequestHandler<Map<String, Object>, ProxyResponse> {

  private static final Pattern ROOT_RE = Pattern.compile("/api/products/?");
  private static final Pattern INDIVIDUAL_RE = Pattern.compile("/api/products/(?<id>\\d+)");
  private static final Pattern HEALTH_RE =
      Pattern.compile("/health/products/(GET|POST|\\d+/GET)");

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
        .or(() -> createOne(stringObjectMap))
        .or(() -> deleteOne(stringObjectMap))
        .or(() -> checkHealth(stringObjectMap))
        .orElse(new ProxyResponse("404", "{\"message\": \"Path not found\"}"));
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
    if (requestIsMatch(stringObjectMap, HEALTH_RE, Constants.GET_METHOD)) {
      return Optional.of(new ProxyResponse("200", "{\"message\": \"OK\"}"));
    }

    return Optional.empty();
  }

  private Optional<ProxyResponse> getAll(@NonNull final Map<String, Object> stringObjectMap) {
    try {
      if (requestIsMatch(stringObjectMap, ROOT_RE, Constants.GET_METHOD)) {
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

      if (requestIsMatch(stringObjectMap, INDIVIDUAL_RE, Constants.GET_METHOD)) {
        final Matcher matcher =
            Optional.ofNullable(stringObjectMap.get("path"))
                .or(() -> Optional.of(""))
                .map(Object::toString)
                .map(INDIVIDUAL_RE::matcher)
                .get();

        matcher.find();

        final String entity = productsController.getOne(
                matcher.group("id"),
                lambdaUtils.getHeader(stringObjectMap, Constants.ACCEPT_HEADER));

        return StringUtils.isNullOrEmpty(entity)
            ? Optional.of(
                 new ProxyResponse("404", "{\"message\": \"Entity not found\"}"))
            : Optional.of(new ProxyResponse("200", entity));
      }
    } catch (final DocumentSerializationException e) {
      return Optional.of(new ProxyResponse("500", e.toString()));
    }

    return Optional.empty();
  }

  private Optional<ProxyResponse> deleteOne(@NonNull final Map<String, Object> stringObjectMap) {
    try {

      if (requestIsMatch(stringObjectMap, INDIVIDUAL_RE, Constants.DELETE_METHOD)) {
        final Matcher matcher =
            Optional.ofNullable(stringObjectMap.get("path"))
                .or(() -> Optional.of(""))
                .map(Object::toString)
                .map(INDIVIDUAL_RE::matcher)
                .get();

        matcher.find();

        final boolean result =
            productsController.delete(
                matcher.group("id"),
                lambdaUtils.getHeader(stringObjectMap, Constants.ACCEPT_HEADER));

        return result
            ? Optional.of(new ProxyResponse("202"))
            : Optional.of(new ProxyResponse("404"));
      }
    } catch (final DocumentSerializationException e) {
      return Optional.of(new ProxyResponse("500", e.toString()));
    }

    return Optional.empty();
  }

  private Optional<ProxyResponse> createOne(@NonNull final Map<String, Object> stringObjectMap) {
    try {
      if (requestIsMatch(stringObjectMap, ROOT_RE, Constants.POST_METHOD)) {
        return Optional.of(
            new ProxyResponse(
                "200",
                productsController.create(
                    getBody(stringObjectMap),
                    lambdaUtils.getHeader(stringObjectMap, Constants.ACCEPT_HEADER))));
      }
    } catch (final DocumentSerializationException e) {
      return Optional.of(
          new ProxyResponse(
              "500",
              "{\"message\": \"" + e + "\", \"body\": \"" + getBody(stringObjectMap) + "\"}"));
    } catch (final RuntimeException ex) {
      System.out.println(ex);
      throw ex;
    }

    return Optional.empty();
  }

  private boolean requestIsMatch(
      @NonNull final Map<String, Object> stringObjectMap,
      @NonNull final Pattern regex,
      @NonNull final String method) {
    final String path = ObjectUtils.defaultIfNull(stringObjectMap.get("path"), "").toString();
    final String requestMethod =
        ObjectUtils.defaultIfNull(stringObjectMap.get("httpMethod"), "").toString().toLowerCase();
    return regex.matcher(path).matches() && method.toLowerCase().equals(requestMethod);
  }

  private String getBody(@NonNull final Map<String, Object> stringObjectMap) {
    final String body = ObjectUtils.defaultIfNull(stringObjectMap.get("body"), "").toString();
    final String isBase64Encoded =
        ObjectUtils.defaultIfNull(stringObjectMap.get("isBase64Encoded"), "")
            .toString()
            .toLowerCase();

    if ("true".equals(isBase64Encoded)) {
      return new String(Base64.getDecoder().decode(body));
    }

    return body;
  }
}
