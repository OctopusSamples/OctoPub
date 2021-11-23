package com.octopus.octopub;

import com.octopus.octopub.models.Product;
import com.octopus.octopub.repositories.ProductRepository;
import com.octopus.octopub.services.LiquidbaseUpdater;
import io.quarkus.test.junit.QuarkusTest;
import java.sql.SQLException;
import javax.inject.Inject;
import javax.transaction.Transactional;
import liquibase.exception.LiquibaseException;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.wildfly.common.Assert;

@QuarkusTest
public class DatabaseTests {

  @Inject
  LiquidbaseUpdater liquidbaseUpdater;

  @Inject
  ProductRepository productRepository;

  @BeforeEach
  public void setup() throws SQLException, LiquibaseException {
    liquidbaseUpdater.update();
  }

  @Test
  @Transactional
  public void createProduct() {
    final Product product = new Product();
    product.setName("test");
    product.setDataPartition("main");
    productRepository.save(product);
    Assert.assertNotNull(product.getId());
  }
}
