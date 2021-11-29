package com.octopus.octopub.providers;

import com.github.jasminb.jsonapi.exceptions.DocumentSerializationException;
import javax.ws.rs.core.Response;
import javax.ws.rs.core.Response.Status;
import javax.ws.rs.ext.ExceptionMapper;
import javax.ws.rs.ext.Provider;
import lombok.NonNull;

/**
 * Converts a DocumentSerializationException to a HTTP response.
 */
@Provider
public class DocumentSerializationExceptionMapper
    implements ExceptionMapper<DocumentSerializationException> {

  @Override
  public Response toResponse(@NonNull final DocumentSerializationException exception) {
    return Response.status(Status.INTERNAL_SERVER_ERROR.getStatusCode(), exception.toString())
        .build();
  }
}
