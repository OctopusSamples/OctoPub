package com.octopus.octopub.providers;

import cz.jirutka.rsql.parser.RSQLParserException;
import javax.ws.rs.core.Response;
import javax.ws.rs.core.Response.Status;
import javax.ws.rs.ext.ExceptionMapper;
import javax.ws.rs.ext.Provider;
import lombok.NonNull;

@Provider
public class RSQLParserExceptionMapper implements ExceptionMapper<RSQLParserException> {

  @Override
  public Response toResponse(@NonNull final RSQLParserException exception) {
    return Response.status(Status.BAD_REQUEST.getStatusCode(), "The supplied filter was invalid")
        .build();
  }
}
