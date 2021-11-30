package com.octopus.octopub.handlers;

import com.github.jasminb.jsonapi.JSONAPIDocument;
import com.github.jasminb.jsonapi.ResourceConverter;
import com.github.jasminb.jsonapi.exceptions.DocumentSerializationException;
import com.octopus.octopub.models.Health;
import javax.enterprise.context.ApplicationScoped;
import javax.inject.Inject;
import lombok.NonNull;

@ApplicationScoped
public class HealthHandler {
  @Inject ResourceConverter resourceConverter;

  public String getHealth(@NonNull final String path, @NonNull final String method)
      throws DocumentSerializationException {
    return respondWithHealth(Health.builder().status("OK").path(path).method(method).build());
  }

  private String respondWithHealth(@NonNull final Health health)
      throws DocumentSerializationException {
    final JSONAPIDocument<Health> document = new JSONAPIDocument<Health>(health);
    return new String(resourceConverter.writeDocument(document));
  }
}
