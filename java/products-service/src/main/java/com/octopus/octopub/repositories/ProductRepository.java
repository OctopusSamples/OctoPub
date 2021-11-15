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

  public List<Product> findAll(@NonNull final String tenant) {
    return em
        .createQuery("Select product from Product product where product.tenant = 'main' or product.tenant = :tenant",
            Product.class)
        .setParameter("tenant", tenant)
        .getResultList();
  }

  public Product save(@NonNull final Product product) {
    product.id = null;
    em.persist(product);
    em.flush();
    return product;
  }
}
