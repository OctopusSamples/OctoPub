package com.octopus.octopub.repositories;

import com.octopus.octopub.models.Product;
import javax.enterprise.context.ApplicationScoped;
import javax.inject.Inject;
import javax.persistence.EntityManager;

@ApplicationScoped
public class ProductRepository {

  @Inject
  EntityManager em;

  public Product findOne(final String id) {
    return em.find(Product.class, urnToInt(id));
  }

  private int urnToInt(final String id) {
    return Integer.parseInt(id.replace(Product.PRODUCT_URN_PREFIX + ":", ""));
  }
}
