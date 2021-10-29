package com.octopus.octopub.repositories;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.JsonMappingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.octopus.octopub.exceptions.ParsingException;
import com.octopus.octopub.models.Audit;
import com.octopus.octopub.services.AuditService;
import java.util.HashMap;
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
        Set the InvocationType header to Event to indicate that the AWS API gateway should fire and forget.
        Azure API Gateway should use send-one-way-request for the audit endpoints.
       */
      auditResource.createAudit(JsonApiConverter.convert(audit), "Event");
    } catch (final Exception ex) {
      /*
        Audits are a best effort creation, explicitly performed asynchronously to maintain
        the performance of the service.
       */
    }
  }
}
