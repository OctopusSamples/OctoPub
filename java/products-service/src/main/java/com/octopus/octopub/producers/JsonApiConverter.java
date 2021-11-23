package com.octopus.octopub.producers;

import com.github.jasminb.jsonapi.DeserializationFeature;
import com.github.jasminb.jsonapi.ResourceConverter;
import com.octopus.octopub.models.Audit;
import com.octopus.octopub.models.Product;
import javax.enterprise.inject.Produces;


public class JsonApiConverter {
  @Produces
  public ResourceConverter buildResourceConverter() {
    final ResourceConverter resourceConverter = new ResourceConverter(Product.class, Audit.class);
    resourceConverter.disableDeserializationOption(DeserializationFeature.REQUIRE_RESOURCE_ID);
    resourceConverter.enableDeserializationOption(DeserializationFeature.ALLOW_UNKNOWN_INCLUSIONS);
    return resourceConverter;
  }
}
