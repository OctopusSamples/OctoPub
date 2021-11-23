package com.octopus.octopub;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.assertThrows;
import static org.junit.jupiter.api.Assertions.assertTrue;

import com.github.jasminb.jsonapi.JSONAPIDocument;
import com.github.jasminb.jsonapi.ResourceConverter;
import com.github.jasminb.jsonapi.exceptions.DocumentSerializationException;
import com.octopus.octopub.exceptions.EntityNotFound;
import com.octopus.octopub.models.Product;
import com.octopus.octopub.services.LiquidbaseUpdater;
import com.octopus.octopub.services.ProductsController;
import io.quarkus.test.junit.QuarkusTest;
import java.nio.charset.StandardCharsets;
import java.sql.SQLException;
import java.util.List;
import javax.inject.Inject;
import javax.transaction.Transactional;
import liquibase.exception.LiquibaseException;
import lombok.NonNull;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.TestInstance;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.ValueSource;

@QuarkusTest
@TestInstance(TestInstance.Lifecycle.PER_CLASS)
public class DatabaseTests {

  @Inject LiquidbaseUpdater liquidbaseUpdater;

  @Inject ProductsController productsController;

  @Inject ResourceConverter resourceConverter;

  @BeforeAll
  public void setup() throws SQLException, LiquibaseException {
    liquidbaseUpdater.update();
  }

  @Test
  @Transactional
  public void createProduct() throws DocumentSerializationException {
    final Product product = new Product();
    product.setName("test");
    product.setDataPartition("main");
    final String result =
        productsController.create(
            productToResourceDocument(product),
            List.of("application/vnd.api+json; dataPartition=testing"));
    final Product resultObject = getProductFromDocument(result);
    assertNotNull(resultObject.getId());
    assertEquals("testing", resultObject.getDataPartition());
  }

  @Test
  @Transactional
  public void updateProduct() throws DocumentSerializationException {
    final Product product = new Product();
    product.setName("test");
    final String result =
        productsController.create(
            productToResourceDocument(product),
            List.of("application/vnd.api+json; dataPartition=testing"));
    final Product resultObject = getProductFromDocument(result);

    final Product product2 = new Product();
    product2.setName("test2");
    final String getResult =
        productsController.update(
            resultObject.getId().toString(),
            productToResourceDocument(product2),
            List.of("application/vnd.api+json; dataPartition=testing"));
    final Product getResultObject = getProductFromDocument(getResult);

    assertNotNull(getResultObject.getId());
    assertEquals("testing", getResultObject.getDataPartition());
    assertEquals("test2", getResultObject.getName());
  }

  /**
   * You should not be able to update a resource in another partition.
   * @param partition The partition to use when updating
   * @throws DocumentSerializationException
   */
  @ParameterizedTest
  @Transactional
  @ValueSource(strings = {"testing2", "", " ", "main", " main ", " testing2 "})
  public void failUpdateProduct(final String partition) throws DocumentSerializationException {
    final Product product = new Product();
    product.setName("test");
    final String result =
        productsController.create(
            productToResourceDocument(product),
            List.of("application/vnd.api+json; dataPartition=testing"));
    final Product resultObject = getProductFromDocument(result);

    final Product product2 = new Product();
    product2.setName("test2");
    assertThrows(EntityNotFound.class, () -> productsController.update(
            resultObject.getId().toString(),
            productToResourceDocument(product2),
            List.of("application/vnd.api+json; dataPartition=" + partition)));
  }

  @Test
  @Transactional
  public void deleteProduct() throws DocumentSerializationException {
    final Product product = new Product();
    product.setName("test");
    final String result =
        productsController.create(
            productToResourceDocument(product),
            List.of("application/vnd.api+json; dataPartition=testing"));
    final Product resultObject = getProductFromDocument(result);

    final boolean success = productsController.delete(
            resultObject.getId().toString(),
            List.of("application/vnd.api+json; dataPartition=testing"));

    assertTrue(success);
  }

  /**
   * You should not be able to delete a resource in another partition.
   * @param partition The partition to use when updating
   * @throws DocumentSerializationException
   */
  @ParameterizedTest
  @Transactional
  @ValueSource(strings = {"testing2", "", " ", "main", " main ", " testing2 "})
  public void failDeleteProduct(final String partition) throws DocumentSerializationException {
    final Product product = new Product();
    product.setName("test");
    final String result =
        productsController.create(
            productToResourceDocument(product),
            List.of("application/vnd.api+json; dataPartition=testing"));
    final Product resultObject = getProductFromDocument(result);

    assertFalse(productsController.delete(
        resultObject.getId().toString(),
        List.of("application/vnd.api+json; dataPartition=" + partition)));
  }

  @Test
  @Transactional
  public void getProduct() throws DocumentSerializationException {
    final Product product = new Product();
    product.setName("test");
    product.setDataPartition("main");
    final String result =
        productsController.create(
            productToResourceDocument(product),
            List.of("application/vnd.api+json; dataPartition=testing"));
    final Product resultObject = getProductFromDocument(result);

    final String getResult =
        productsController.getOne(
            resultObject.getId().toString(),
            List.of("application/vnd.api+json; dataPartition=testing"));
    final Product getResultObject = getProductFromDocument(getResult);

    assertEquals(resultObject.getId(), getResultObject.getId());
    assertEquals(resultObject.getName(), getResultObject.getName());
    assertEquals(resultObject.getDataPartition(), getResultObject.getDataPartition());
  }

  /**
   * You should not be able to get a resource in another partition.
   * @param partition The partition to use when retrieving
   * @throws DocumentSerializationException
   */
  @ParameterizedTest
  @Transactional
  @ValueSource(strings = {"testing2", "", " ", "main", " main ", " testing2 "})
  public void failGetProduct(final String partition) throws DocumentSerializationException {
    final Product product = new Product();
    product.setName("test");
    final String result =
        productsController.create(
            productToResourceDocument(product),
            List.of("application/vnd.api+json; dataPartition=testing"));
    final Product resultObject = getProductFromDocument(result);

    assertThrows(EntityNotFound.class, () -> productsController.getOne(
        resultObject.getId().toString(),
        List.of("application/vnd.api+json; dataPartition=" + partition)));
  }

  @Test
  @Transactional
  public void getAllProduct() throws DocumentSerializationException {
    final Product product = new Product();
    product.setName("test");
    product.setDataPartition("main");
    final String result =
        productsController.create(
            productToResourceDocument(product),
            List.of("application/vnd.api+json; dataPartition=testing"));
    final Product resultObject = getProductFromDocument(result);

    final String getResult =
        productsController.getAll(
            List.of("application/vnd.api+json; dataPartition=testing"),
            "id==" + resultObject.getId());
    final List<Product> getResultObjects = getProductsFromDocument(getResult);

    assertEquals(1, getResultObjects.size());
    assertEquals(resultObject.getId(), getResultObjects.get(0).getId());
    assertEquals(resultObject.getName(), getResultObjects.get(0).getName());
    assertEquals(resultObject.getDataPartition(), getResultObjects.get(0).getDataPartition());
  }

  /**
   * You should not be able to list resources in another partition.
   * @param partition The partition to use when retrieving
   * @throws DocumentSerializationException
   */
  @ParameterizedTest
  @Transactional
  @ValueSource(strings = {"testing2", "", " ", "main", " main ", " testing2 "})
  public void failGetProducts(final String partition) throws DocumentSerializationException {
    final Product product = new Product();
    product.setName("test");
    final String result =
        productsController.create(
            productToResourceDocument(product),
            List.of("application/vnd.api+json; dataPartition=testing"));
    final Product resultObject = getProductFromDocument(result);

    final String getResult = productsController.getAll(
        List.of("application/vnd.api+json; dataPartition=" + partition),
        "");
    final List<Product> getResultObjects = getProductsFromDocument(getResult);

    assertFalse(getResultObjects.stream().anyMatch(p -> p.getId() == resultObject.getId()));
  }

  private Product getProductFromDocument(@NonNull final String document) {
    final JSONAPIDocument<Product> productDocument =
        resourceConverter.readDocument(document.getBytes(StandardCharsets.UTF_8), Product.class);
    final Product product = productDocument.get();
    return product;
  }

  private List<Product> getProductsFromDocument(@NonNull final String document) {
    final JSONAPIDocument<List<Product>> productDocument =
        resourceConverter.readDocumentCollection(
            document.getBytes(StandardCharsets.UTF_8), Product.class);
    final List<Product> products = productDocument.get();
    return products;
  }

  private String productToResourceDocument(@NonNull final Product product)
      throws DocumentSerializationException {
    final JSONAPIDocument<Product> document = new JSONAPIDocument<Product>(product);
    return new String(resourceConverter.writeDocument(document));
  }
}
