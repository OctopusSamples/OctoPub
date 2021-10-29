package com.octopus.octopub.repositories;

import com.octopus.octopub.models.Product;
import java.util.List;
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

  public List<Product> findAll() {
    return em
        .createQuery("Select product from Product product", Product.class)
        .getResultList();
  }

  private int urnToInt(final String id) {
    return Integer.parseInt(id.replace(Product.PRODUCT_URN_PREFIX + ":", ""));
  }
}
