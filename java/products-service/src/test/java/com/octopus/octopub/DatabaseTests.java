package com.octopus.octopub;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.assertThrows;
import static org.junit.jupiter.api.Assertions.assertTrue;

import com.github.jasminb.jsonapi.ResourceConverter;
import com.github.jasminb.jsonapi.exceptions.DocumentSerializationException;
import com.octopus.octopub.exceptions.EntityNotFound;
import com.octopus.octopub.models.Product;
import com.octopus.octopub.services.LiquidbaseUpdater;
import com.octopus.octopub.handlers.ProductsHandler;
import io.quarkus.test.junit.QuarkusTest;
import java.sql.SQLException;
import java.util.List;
import javax.inject.Inject;
import javax.transaction.Transactional;
import liquibase.exception.LiquibaseException;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.TestInstance;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.ValueSource;

@QuarkusTest
@TestInstance(TestInstance.Lifecycle.PER_CLASS)
public class DatabaseTests extends BaseTest {

  @Inject LiquidbaseUpdater liquidbaseUpdater;

  @Inject
  ProductsHandler productsHandler;

  @Inject ResourceConverter resourceConverter;

  @BeforeAll
  public void setup() throws SQLException, LiquibaseException {
    liquidbaseUpdater.update();
  }

  @Test
  @Transactional
  public void testCreateProduct() throws DocumentSerializationException {
    final Product resultObject = createProduct(productsHandler, resourceConverter, "testing");
    assertNotNull(resultObject.getId());
    assertEquals("testing", resultObject.getDataPartition());
    assertEquals("name", resultObject.getName());
    assertEquals("a test book", resultObject.getDescription());
    assertEquals("http://example.org/epub", resultObject.getEpub());
    assertEquals("http://example.org/pdf", resultObject.getPdf());
    assertEquals("http://example.org/image", resultObject.getImage());
  }

  @Test
  @Transactional
  public void updateProduct() throws DocumentSerializationException {
    final Product product = new Product();
    product.setName("test");
    product.setDescription("a test book");
    product.setEpub("http://example.org");
    product.setPdf("http://example.org");
    product.setImage("http://example.org");
    final String result =
        productsHandler.create(
            productToResourceDocument(resourceConverter, product),
            List.of("application/vnd.api+json; dataPartition=testing"));
    final Product resultObject = getProductFromDocument(resourceConverter, result);

    final Product product2 = new Product();
    product2.setName("test2");
    product2.setDescription("a test book updated");
    product2.setEpub("http://example.org/updatedEpub");
    product2.setPdf("http://example.org/updatedPdf");
    product2.setImage("http://example.org/updatedImage");
    final String getResult =
        productsHandler.update(
            resultObject.getId().toString(),
            productToResourceDocument(resourceConverter, product2),
            List.of("application/vnd.api+json; dataPartition=testing"));
    final Product getResultObject = getProductFromDocument(resourceConverter, getResult);

    assertNotNull(getResultObject.getId());
    assertEquals("testing", getResultObject.getDataPartition());
    assertEquals("a test book updated", getResultObject.getDescription());
    assertEquals("http://example.org/updatedEpub", getResultObject.getEpub());
    assertEquals("http://example.org/updatedPdf", getResultObject.getPdf());
    assertEquals("http://example.org/updatedImage", getResultObject.getImage());
  }

  /**
   * You should not be able to update a resource in another partition.
   *
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
        productsHandler.create(
            productToResourceDocument(resourceConverter, product),
            List.of("application/vnd.api+json; dataPartition=testing"));
    final Product resultObject = getProductFromDocument(resourceConverter, result);

    final Product product2 = new Product();
    product2.setName("test2");
    assertThrows(
        EntityNotFound.class,
        () ->
            productsHandler.update(
                resultObject.getId().toString(),
                productToResourceDocument(resourceConverter, product2),
                List.of("application/vnd.api+json; dataPartition=" + partition)));

    // verify the same exceptions occur when no header is set
    assertThrows(
        EntityNotFound.class,
        () ->
            productsHandler.update(
                resultObject.getId().toString(), productToResourceDocument(resourceConverter, product2), List.of()));
  }

  @Test
  @Transactional
  public void deleteProduct() throws DocumentSerializationException {
    final Product product = new Product();
    product.setName("test");
    final String result =
        productsHandler.create(
            productToResourceDocument(resourceConverter, product),
            List.of("application/vnd.api+json; dataPartition=testing"));
    final Product resultObject = getProductFromDocument(resourceConverter, result);

    final boolean success =
        productsHandler.delete(
            resultObject.getId().toString(),
            List.of("application/vnd.api+json; dataPartition=testing"));

    assertTrue(success);
  }

  /**
   * You should not be able to delete a resource in another partition.
   *
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
        productsHandler.create(
            productToResourceDocument(resourceConverter, product),
            List.of("application/vnd.api+json; dataPartition=testing"));
    final Product resultObject = getProductFromDocument(resourceConverter, result);

    assertFalse(
        productsHandler.delete(
            resultObject.getId().toString(),
            List.of("application/vnd.api+json; dataPartition=" + partition)));

    assertFalse(productsHandler.delete(resultObject.getId().toString(), List.of()));
  }

  @Test
  @Transactional
  public void getProduct() throws DocumentSerializationException {
    final Product product = new Product();
    product.setName("test");
    product.setDataPartition("main");
    final String result =
        productsHandler.create(
            productToResourceDocument(resourceConverter, product),
            List.of("application/vnd.api+json; dataPartition=testing"));
    final Product resultObject = getProductFromDocument(resourceConverter, result);

    final String getResult =
        productsHandler.getOne(
            resultObject.getId().toString(),
            List.of("application/vnd.api+json; dataPartition=testing"));
    final Product getResultObject = getProductFromDocument(resourceConverter, getResult);

    assertEquals(resultObject.getId(), getResultObject.getId());
    assertEquals(resultObject.getName(), getResultObject.getName());
    assertEquals(resultObject.getDataPartition(), getResultObject.getDataPartition());
  }

  /**
   * You should not be able to get a resource in another partition.
   *
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
        productsHandler.create(
            productToResourceDocument(resourceConverter, product),
            List.of("application/vnd.api+json; dataPartition=testing"));
    final Product resultObject = getProductFromDocument(resourceConverter, result);

    assertThrows(
        EntityNotFound.class,
        () ->
            productsHandler.getOne(
                resultObject.getId().toString(),
                List.of("application/vnd.api+json; dataPartition=" + partition)));

    assertThrows(
        EntityNotFound.class,
        () -> productsHandler.getOne(resultObject.getId().toString(), List.of()));
  }

  @Test
  @Transactional
  public void getAllProduct() throws DocumentSerializationException {
    final Product product = new Product();
    product.setName("test");
    product.setDataPartition("main");
    final String result =
        productsHandler.create(
            productToResourceDocument(resourceConverter, product),
            List.of("application/vnd.api+json; dataPartition=testing"));
    final Product resultObject = getProductFromDocument(resourceConverter, result);

    final String getResult =
        productsHandler.getAll(
            List.of("application/vnd.api+json; dataPartition=testing"),
            "id==" + resultObject.getId());
    final List<Product> getResultObjects = getProductsFromDocument(resourceConverter, getResult);

    assertEquals(1, getResultObjects.size());
    assertEquals(resultObject.getId(), getResultObjects.get(0).getId());
    assertEquals(resultObject.getName(), getResultObjects.get(0).getName());
    assertEquals(resultObject.getDataPartition(), getResultObjects.get(0).getDataPartition());
  }

  /**
   * You should not be able to list resources in another partition.
   *
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
        productsHandler.create(
            productToResourceDocument(resourceConverter, product),
            List.of("application/vnd.api+json; dataPartition=testing"));
    final Product resultObject = getProductFromDocument(resourceConverter, result);

    final String getResult =
        productsHandler.getAll(
            List.of("application/vnd.api+json; dataPartition=" + partition), "");
    final List<Product> getResultObjects = getProductsFromDocument(resourceConverter, getResult);

    assertFalse(getResultObjects.stream().anyMatch(p -> p.getId() == resultObject.getId()));

    final String getResult2 = productsHandler.getAll(List.of(), "");
    final List<Product> getResultObjects2 = getProductsFromDocument(resourceConverter, getResult2);

    assertFalse(getResultObjects2.stream().anyMatch(p -> p.getId() == resultObject.getId()));
  }


}
