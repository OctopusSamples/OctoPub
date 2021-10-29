package com.octopus.octopub.repositories;

import com.octopus.octopub.Constants;
import com.octopus.octopub.models.Audit;
import com.octopus.octopub.services.AuditService;
import javax.enterprise.context.ApplicationScoped;
import nl.michelbijnen.jsonapi.parser.JsonApiConverter;
import org.eclipse.microprofile.rest.client.inject.RestClient;

@ApplicationScoped
public class AuditRepository {

  @RestClient
  AuditService auditResource;

  public void save(final Audit audit) {
    try {
      /*
        AWS API Gateway should use the Event invocation type.
        Azure API Gateway should use send-one-way-request for the audit endpoints.
       */
      auditResource.createAudit(
          JsonApiConverter.convert(audit),
          Constants.JSONAPI_CONTENT_TYPE);
    } catch (final Exception ex) {
      /*
        Audits are a best effort creation, explicitly performed asynchronously to maintain
        the performance of the service.
       */
    }
  }
}
