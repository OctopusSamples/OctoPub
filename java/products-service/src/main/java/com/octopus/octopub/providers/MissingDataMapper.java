package com.octopus.octopub.providers;

import com.octopus.octopub.exceptions.InvalidInput;
import com.octopus.octopub.exceptions.MissingData;
import javax.ws.rs.core.Response;
import javax.ws.rs.core.Response.Status;
import javax.ws.rs.ext.ExceptionMapper;
import javax.ws.rs.ext.Provider;
import lombok.NonNull;

@Provider
public class MissingDataMapper implements ExceptionMapper<MissingData> {

  @Override
  public Response toResponse(@NonNull final MissingData exception) {
    return Response.status(Status.BAD_REQUEST.getStatusCode(),
        "The supplied resource did not include a data object").build();
  }
}
