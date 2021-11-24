package com.octopus.octopub.providers;

import com.octopus.octopub.exceptions.InvalidAcceptHeaders;
import com.octopus.octopub.exceptions.MissingData;
import javax.ws.rs.core.Response;
import javax.ws.rs.core.Response.Status;
import javax.ws.rs.ext.ExceptionMapper;
import javax.ws.rs.ext.Provider;
import lombok.NonNull;

@Provider
public class InvalidAcceptHeadersMapper implements ExceptionMapper<InvalidAcceptHeaders> {

  @Override
  public Response toResponse(@NonNull final InvalidAcceptHeaders exception) {
    return Response.status(Status.NOT_ACCEPTABLE.getStatusCode()).build();
  }
}
