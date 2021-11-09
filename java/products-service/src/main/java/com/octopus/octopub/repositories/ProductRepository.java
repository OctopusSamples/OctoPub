package com.octopus.octopub.repositories;

import com.octopus.octopub.models.Product;
import java.util.List;
import javax.enterprise.context.ApplicationScoped;
import javax.inject.Inject;
import javax.persistence.EntityManager;
import lombok.NonNull;

@ApplicationScoped
public class ProductRepository {

  @Inject
  EntityManager em;

  public Product findOne(final int id) {
    return em.find(Product.class, id);
  }

  public List<Product> findAll() {
    return em
        .createQuery("Select product from Product product", Product.class)
        .getResultList();
  }

  public Product save(@NonNull final Product product) {
    em.persist(product);
    return product;
  }
}
