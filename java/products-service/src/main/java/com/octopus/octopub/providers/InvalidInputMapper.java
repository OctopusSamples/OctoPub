package com.octopus.octopub.providers;

import com.octopus.octopub.exceptions.InvalidInput;
import javax.ws.rs.core.Response;
import javax.ws.rs.core.Response.Status;
import javax.ws.rs.ext.ExceptionMapper;
import javax.ws.rs.ext.Provider;
import lombok.NonNull;

@Provider
public class InvalidInputMapper implements ExceptionMapper<InvalidInput> {

  @Override
  public Response toResponse(@NonNull final InvalidInput exception) {
    return Response.status(Status.BAD_REQUEST.getStatusCode(), "The supplied resource was invalid")
        .build();
  }
}
