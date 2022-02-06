package com.octopus.octopub;

import com.github.jasminb.jsonapi.JSONAPIDocument;
import com.github.jasminb.jsonapi.ResourceConverter;
import com.github.jasminb.jsonapi.exceptions.DocumentSerializationException;
import com.octopus.octopub.domain.entities.Product;
import com.octopus.octopub.domain.handlers.ProductsHandler;
import java.nio.charset.StandardCharsets;
import java.util.List;
import javax.annotation.Nonnull;
import lombok.NonNull;

public class BaseTest {
  protected Product createProduct(@NonNull final String name) {
    return createProduct(name, null);
  }

  protected Product createProduct(@NonNull final String name, final String partition) {
    final Product product = new Product();
    product.setName(name);
    product.setDataPartition(partition);
    product.setDescription("a test book");
    product.setEpub("http://example.org/epub");
    product.setPdf("http://example.org/pdf");
    product.setImage("http://example.org/image");
    return product;
  }

  protected Product createProduct(
      @Nonnull final ProductsHandler productsHandler,
      @NonNull final ResourceConverter resourceConverter,
      @Nonnull final String partition)
      throws DocumentSerializationException {
    final Product product = createProduct("name");
    final String result =
        productsHandler.create(
            productToResourceDocument(resourceConverter, product),
            List.of("application/vnd.api+json; dataPartition=" + partition),
            null);
    final Product resultObject = getProductFromDocument(resourceConverter, result);
    return resultObject;
  }

  protected Product getProductFromDocument(
      @NonNull final ResourceConverter resourceConverter, @NonNull final String document) {
    final JSONAPIDocument<Product> productDocument =
        resourceConverter.readDocument(document.getBytes(StandardCharsets.UTF_8), Product.class);
    final Product product = productDocument.get();
    return product;
  }

  protected List<Product> getProductsFromDocument(
      @NonNull final ResourceConverter resourceConverter, @NonNull final String document) {
    final JSONAPIDocument<List<Product>> productDocument =
        resourceConverter.readDocumentCollection(
            document.getBytes(StandardCharsets.UTF_8), Product.class);
    final List<Product> products = productDocument.get();
    return products;
  }

  protected String productToResourceDocument(
      @NonNull final ResourceConverter resourceConverter, @NonNull final Product product)
      throws DocumentSerializationException {
    final JSONAPIDocument<Product> document = new JSONAPIDocument<Product>(product);
    return new String(resourceConverter.writeDocument(document));
  }
}
