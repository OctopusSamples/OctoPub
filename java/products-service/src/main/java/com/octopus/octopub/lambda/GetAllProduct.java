package com.octopus.octopub.lambda;

import com.amazonaws.services.lambda.runtime.Context;
import com.amazonaws.services.lambda.runtime.RequestHandler;
import com.github.jasminb.jsonapi.exceptions.DocumentSerializationException;
import com.octopus.octopub.services.LambdaUtils;
import com.octopus.octopub.services.ProductsController;
import java.util.Map;
import javax.inject.Inject;
import javax.inject.Named;
import javax.transaction.Transactional;

@Named("GetAll")
public class GetAllProduct implements RequestHandler<Map<String, Object>, ProxyResponse> {

  @Inject
  ProductsController productsController;

  @Inject
  LambdaUtils lambdaUtils;

  /**
   * See https://github.com/quarkusio/quarkus/issues/5811 for why we need @Transactional.
   *
   * @param stringObjectMap The request details
   * @param context         The request context
   * @return The proxy response
   */
  @Override
  @Transactional
  public ProxyResponse handleRequest(final Map<String, Object> stringObjectMap,
      final Context context) {
    try {
      return new ProxyResponse(
          "200",
          productsController.getAll(lambdaUtils.getHeader(stringObjectMap, "Accept")));
    } catch (final DocumentSerializationException e) {
      return new ProxyResponse("500", e.toString());
    }
  }
}
