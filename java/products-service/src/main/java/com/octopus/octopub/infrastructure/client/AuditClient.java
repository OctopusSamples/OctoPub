package com.octopus.octopub.infrastructure.client;

import com.octopus.octopub.application.Constants;
import javax.ws.rs.Consumes;
import javax.ws.rs.HeaderParam;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import org.eclipse.microprofile.rest.client.inject.RegisterRestClient;

/** A REST client to access the audits service. */
@Path("api")
@RegisterRestClient
public interface AuditClient {
  @Path("audits")
  @POST
  @Consumes(Constants.JSON_CONTENT_TYPE)
  String createAudit(
      final String audit,
      @HeaderParam(Constants.ACCEPT_HEADER) String accept,
      @HeaderParam(Constants.API_GATEWAY_API_KEY_HEADER) String apiKey);
}
