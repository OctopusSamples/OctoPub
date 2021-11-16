package com.octopus.octopub.services;

import javax.ws.rs.HeaderParam;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import org.eclipse.microprofile.rest.client.inject.RegisterRestClient;

@Path("api")
@RegisterRestClient
public interface AuditService {
  @Path("audits")
  @POST
  void createAudit(
      final String audit,
      @HeaderParam("Accept") String accept,
      @HeaderParam("X-Amz-Invocation-Type") String invocationType);
}
