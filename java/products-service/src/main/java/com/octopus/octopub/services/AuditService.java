package com.octopus.octopub.services;

import com.octopus.octopub.Constants;
import java.util.List;
import javax.ws.rs.Consumes;
import javax.ws.rs.HeaderParam;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import org.eclipse.microprofile.rest.client.inject.RegisterRestClient;

@Path("api")
@RegisterRestClient
public interface AuditService {
  @Path("audits")
  @POST
  @Consumes(Constants.JSON_CONTENT_TYPE)
  String createAudit(
      final String audit,
      @HeaderParam(Constants.ACCEPT_HEADER) String accept,
      @HeaderParam(Constants.API_GATEWAY_API_KEY_HEADER) String apiKey);
}
