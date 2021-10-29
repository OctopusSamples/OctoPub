package com.octopus.octopub.repositories;

import com.octopus.octopub.models.Product;
import io.crnk.data.jpa.JpaEntityRepositoryBase;

public class ProductRepository extends JpaEntityRepositoryBase<Product, String> {
  public ProductRepository() {
    super(Product.class);
  }
}
