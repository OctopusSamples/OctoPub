package com.octopus.octopub.repositories;

import com.github.jasminb.jsonapi.JSONAPIDocument;
import com.octopus.octopub.Constants;
import com.octopus.octopub.models.Audit;
import com.octopus.octopub.models.Product;
import com.octopus.octopub.services.AuditService;
import com.octopus.octopub.services.JsonApiConverter;
import javax.enterprise.context.ApplicationScoped;
import javax.inject.Inject;
import javax.ws.rs.core.Response;
import lombok.NonNull;
import org.eclipse.microprofile.rest.client.inject.RestClient;

@ApplicationScoped
public class AuditRepository {

  @RestClient
  AuditService auditResource;

  @Inject
  JsonApiConverter jsonApiConverter;

  public void save(final Audit audit) {
    try {
      final JSONAPIDocument<Audit> document = new JSONAPIDocument<Audit>(audit);

      /*
        AWS API Gateway should use the Event invocation type.
        Azure API Gateway should use send-one-way-request for the audit endpoints.
       */
      auditResource.createAudit(
          new String(jsonApiConverter.buildResourceConverter().writeDocument(document)),
          Constants.JSONAPI_CONTENT_TYPE);
    } catch (final Exception ex) {
      /*
        Audits are a best effort creation, explicitly performed asynchronously to maintain
        the performance of the service.
       */
    }
  }
}
