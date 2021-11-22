package com.octopus.octopub.providers;

import com.octopus.octopub.exceptions.EntityNotFound;
import com.octopus.octopub.exceptions.MissingData;
import javax.ws.rs.core.Response;
import javax.ws.rs.core.Response.Status;
import javax.ws.rs.ext.ExceptionMapper;
import javax.ws.rs.ext.Provider;
import lombok.NonNull;

@Provider
public class EntityNotFoundMapper implements ExceptionMapper<EntityNotFound> {

  @Override
  public Response toResponse(@NonNull final EntityNotFound exception) {
    return Response.status(Status.NOT_FOUND.getStatusCode(),
        "The request resource was not found").build();
  }
}
