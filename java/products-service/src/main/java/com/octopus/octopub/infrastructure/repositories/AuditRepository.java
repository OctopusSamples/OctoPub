package com.octopus.octopub.infrastructure.repositories;

import com.github.jasminb.jsonapi.JSONAPIDocument;
import com.octopus.octopub.GlobalConstants;
import com.octopus.octopub.domain.entities.Audit;
import com.octopus.octopub.domain.framework.producers.JsonApiConverter;
import com.octopus.octopub.infrastructure.client.AuditClient;
import java.util.List;
import java.util.Optional;
import javax.enterprise.context.ApplicationScoped;
import javax.inject.Inject;
import lombok.NonNull;
import org.eclipse.microprofile.config.inject.ConfigProperty;
import org.eclipse.microprofile.rest.client.inject.RestClient;
import org.jboss.logging.Logger;

/** This repository is used to create new audit records in another microservice. */
@ApplicationScoped
public class AuditRepository {

  @Inject Logger log;

  @RestClient
  AuditClient auditResource;

  @Inject JsonApiConverter jsonApiConverter;

  @ConfigProperty(name = "infrastructure.api-key")
  Optional<String> apiKey;

  /**
   * Creates an audit record.
   *
   * @param audit The audit record to create.
   * @param acceptHeaders The "Accept" headers.
   */
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
          apiKey.orElse(""));
    } catch (final Exception ex) {
      log.error(GlobalConstants.MICROSERVICE_NAME + "-Network-AuditCreateFailed", ex);
      /*
       Audits are a best effort creation, explicitly performed asynchronously to maintain
       the performance of the service. Sagas should be used if the failure of an audit event
       reverses the changes to a product.
      */
    }
  }
}
