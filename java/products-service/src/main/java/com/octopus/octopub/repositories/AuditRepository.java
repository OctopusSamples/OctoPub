package com.octopus.octopub.repositories;

import com.github.jasminb.jsonapi.JSONAPIDocument;
import com.octopus.octopub.Constants;
import com.octopus.octopub.models.Audit;
import com.octopus.octopub.services.AuditService;
import com.octopus.octopub.producers.JsonApiConverter;
import java.util.List;
import java.util.Optional;
import javax.enterprise.context.ApplicationScoped;
import javax.inject.Inject;
import lombok.NonNull;
import org.eclipse.microprofile.config.inject.ConfigProperty;
import org.eclipse.microprofile.rest.client.inject.RestClient;

@ApplicationScoped
public class AuditRepository {

  @RestClient
  AuditService auditResource;

  @Inject
  JsonApiConverter jsonApiConverter;

  @ConfigProperty(name = "infrastructure.api-key")
  Optional<String> apiKey;

  public void save(@NonNull final Audit audit, @NonNull final List<String> acceptHeaders) {
    try {
      final JSONAPIDocument<Audit> document = new JSONAPIDocument<Audit>(audit);

      /*
        AWS API Gateway should use the Event invocation type.
        Azure API Gateway should use send-one-way-request for the audit endpoints.
       */
      auditResource.createAudit(
          new String(jsonApiConverter.buildResourceConverter().writeDocument(document)),
          String.join(",", acceptHeaders),
          Constants.EVENT_INVOCATION,
          apiKey.orElse(""));
    } catch (final Exception ex) {
      System.out.println("AuditRepository.save(Audit, List<String>): " + ex);
      /*
        Audits are a best effort creation, explicitly performed asynchronously to maintain
        the performance of the service. Sagas should be used if the failure of an audit event
        reverses the changes to a product.
       */
    }
  }
}
