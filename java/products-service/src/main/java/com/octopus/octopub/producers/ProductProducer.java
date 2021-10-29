package com.octopus.octopub.producers;

import com.octopus.octopub.repositories.ProductRepository;
import javax.enterprise.context.ApplicationScoped;
import javax.enterprise.inject.Produces;

public class ProductProducer {
  @Produces
  @ApplicationScoped
  public ProductRepository productRepository() {
    return new ProductRepository();
  }
}
