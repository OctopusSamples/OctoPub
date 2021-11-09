package com.octopus.octopub.lambda;

import com.amazonaws.services.lambda.runtime.Context;
import com.amazonaws.services.lambda.runtime.RequestHandler;
import java.util.Map;

public class Lambda implements RequestHandler<Map<String, Object>, ProxyResponse> {
  @Override
  public ProxyResponse handleRequest(final Map<String, Object> stringObjectMap, final Context context) {
    return new ProxyResponse("200", "ok");
  }
}
