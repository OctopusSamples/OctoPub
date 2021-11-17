package com.octopus.octopub.services;

import com.octopus.octopub.Constants;
import javax.ws.rs.Consumes;
import javax.ws.rs.HeaderParam;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import org.eclipse.microprofile.rest.client.inject.RegisterRestClient;

@Path("message")
@RegisterRestClient
public interface AuditService {
  @Path("audits")
  @POST
  @Consumes(Constants.JSON_CONTENT_TYPE)
  String createAudit(
      final String audit,
      @HeaderParam("X-Amz-Invocation-Type") String invocationType,
      @HeaderParam("X-API-Key") String apiKey);
}
