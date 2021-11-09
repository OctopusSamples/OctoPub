package com.octopus.octopub.services;

import com.github.jasminb.jsonapi.ResourceConverter;
import com.octopus.octopub.models.Audit;
import com.octopus.octopub.models.Product;
import javax.enterprise.context.ApplicationScoped;

@ApplicationScoped
public class JsonApiConverter {
  public ResourceConverter buildResourceConverter() {
    return new ResourceConverter(Product.class, Audit.class);
  }
}
