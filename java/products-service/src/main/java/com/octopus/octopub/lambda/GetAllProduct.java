package com.octopus.octopub.lambda;

import com.amazonaws.services.lambda.runtime.Context;
import com.amazonaws.services.lambda.runtime.RequestHandler;
import com.github.jasminb.jsonapi.exceptions.DocumentSerializationException;
import com.octopus.octopub.Constants;
import com.octopus.octopub.services.LambdaUtils;
import com.octopus.octopub.services.ProductsController;
import java.util.Map;
import java.util.Optional;
import java.util.regex.Matcher;
import java.util.regex.Pattern;
import javax.inject.Inject;
import javax.inject.Named;
import javax.transaction.Transactional;
import lombok.NonNull;

@Named("GetAll")
public class GetAllProduct implements RequestHandler<Map<String, Object>, ProxyResponse> {

  private static final Pattern ROOT_RE = Pattern.compile("^/api/products/?$");
  private static final Pattern INDIVIDUAL_RE = Pattern.compile("^/api/products/(?<id>\\d+)$");

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
      final Map<String, Object> stringObjectMap, final Context context) {

    return getAll(stringObjectMap)
        .or(() -> getOne(stringObjectMap))
        .orElse(new ProxyResponse("404", "Path not found"));
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
