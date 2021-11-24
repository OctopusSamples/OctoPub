package com.octopus.octopub.handlers;

import com.github.jasminb.jsonapi.JSONAPIDocument;
import com.github.jasminb.jsonapi.ResourceConverter;
import com.github.jasminb.jsonapi.exceptions.DocumentSerializationException;
import com.octopus.octopub.Constants;
import com.octopus.octopub.exceptions.EntityNotFound;
import com.octopus.octopub.exceptions.InvalidInput;
import com.octopus.octopub.exceptions.MissingData;
import com.octopus.octopub.models.Audit;
import com.octopus.octopub.models.Product;
import com.octopus.octopub.repositories.AuditRepository;
import com.octopus.octopub.repositories.ProductRepository;
import com.octopus.octopub.services.PartitionIdentifier;
import java.nio.charset.StandardCharsets;
import java.util.List;
import javax.enterprise.context.ApplicationScoped;
import javax.inject.Inject;
import lombok.NonNull;

/**
 * Handlers take the raw input from the upstream service, like Lambda or a web server,
 * convert the inputs to POJOs, apply the security rules, create an audit trail, and then pass
 * the requests down to repositories.
 */
@ApplicationScoped
public class ProductsHandler {

  @Inject
  ProductRepository productRepository;

  @Inject
  AuditRepository auditRepository;

  @Inject
  ResourceConverter resourceConverter;

  @Inject
  PartitionIdentifier partitionIdentifier;

  public String getAll(@NonNull final List<String> acceptHeaders, final String filterParam)
      throws DocumentSerializationException {
    final List<Product> products =
        productRepository.findAll(
            List.of(Constants.DEFAULT_PARTITION, partitionIdentifier.getPartition(acceptHeaders)),
            filterParam);
    final JSONAPIDocument<List<Product>> document = new JSONAPIDocument<List<Product>>(products);
    final byte[] content = resourceConverter.writeDocumentCollection(document);
    return new String(content);
  }

  public String create(@NonNull final String document, @NonNull final List<String> acceptHeaders)
      throws DocumentSerializationException {
    final Product product = getProductFromDocument(document);

    if (product == null) {
      throw new MissingData();
    }

    product.dataPartition = partitionIdentifier.getPartition(acceptHeaders);
    productRepository.save(product);
    auditRepository.save(
        new Audit(
            Constants.MICROSERVICE_NAME,
            Constants.CREATED_ACTION,
            "Product-" + product.getId().toString()),
        acceptHeaders);

    return respondWithProduct(product);
  }

  public String update(
      @NonNull final String id,
      @NonNull final String document,
      @NonNull final List<String> acceptHeaders)
      throws DocumentSerializationException {
    final Product product = getProductFromDocument(document);

    // The input was missing the required data
    if (product == null) {
      throw new MissingData();
    }

    try {
      final Integer intId = Integer.parseInt(id);
      // find the product we are updating
      final Product existingProduct = productRepository.findOne(intId);

      if (existingProduct != null) {
        // the existing product must have the same partition as the current request to be updated
        if (partitionIdentifier.getPartition(acceptHeaders).equals(existingProduct.dataPartition)) {
          // update the product details
          final Product updated = productRepository.update(product, intId);

          // Create an audit record noting the change
          auditRepository.save(
              new Audit(
                  Constants.MICROSERVICE_NAME,
                  Constants.UPDATED_ACTION,
                  "Product-" + intId),
              acceptHeaders);

          return respondWithProduct(updated);

        } else {
          // Create an audit record noting the failure
          auditRepository.save(
              new Audit(
                  Constants.MICROSERVICE_NAME,
                  Constants.UPDATED_FAILED_PARTITION_MISMATCH_ACTION,
                  "Product-" + intId),
              acceptHeaders);

          // Don't update the resource, and flow through to return a 404 not found error
        }
      }
    } catch (final NumberFormatException ex) {
      // ignored, as the supplied id was not an int, and would never find any entities
    }

    throw new EntityNotFound();
  }

  public String getOne(@NonNull final String id, @NonNull final List<String> acceptHeaders)
      throws DocumentSerializationException {
    try {
      final Product product = productRepository.findOne(Integer.parseInt(id));
      if (product != null
          && (Constants.DEFAULT_PARTITION.equals(product.getDataPartition())
          || partitionIdentifier.getPartition(acceptHeaders).equals(product.getDataPartition()))) {
        return respondWithProduct(product);
      }
    } catch (final NumberFormatException ex) {
      // ignored, as the supplied id was not an int, and would never find any entities
    }
    throw new EntityNotFound();
  }

  public boolean delete(@NonNull final String id, @NonNull final List<String> acceptHeaders) {
    try {
      final Integer intId = Integer.parseInt(id);
      final Product product = productRepository.findOne(intId);
      // The product being deleted must match the current partition
      if (product != null
          && partitionIdentifier.getPartition(acceptHeaders).equals(product.getDataPartition())) {
        productRepository.delete(intId);
        auditRepository.save(
            new Audit(Constants.MICROSERVICE_NAME, Constants.DELETED_ACTION, "Product-" + intId),
            acceptHeaders);
        return true;
      }
    } catch (final NumberFormatException ex) {
      // ignored, as the supplied id was not an int, and would never find any entities
    }

    return false;
  }

  private Product getProductFromDocument(@NonNull final String document) {
    final JSONAPIDocument<Product> productDocument =
        resourceConverter.readDocument(document.getBytes(StandardCharsets.UTF_8), Product.class);
    final Product product = productDocument.get();
    /*
      The ID of a product is determined by the URL, while the partition comes froms
      the headers. If either of these values was sent by the client, strip them out.
     */
    product.id = null;
    product.dataPartition = null;
    return product;
  }

  private String respondWithProduct(@NonNull final Product product)
      throws DocumentSerializationException {
    final JSONAPIDocument<Product> document = new JSONAPIDocument<Product>(product);
    return new String(resourceConverter.writeDocument(document));
  }
}