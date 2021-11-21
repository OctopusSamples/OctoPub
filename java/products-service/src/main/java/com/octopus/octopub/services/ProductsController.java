package com.octopus.octopub.services;

import com.github.jasminb.jsonapi.JSONAPIDocument;
import com.github.jasminb.jsonapi.ResourceConverter;
import com.github.jasminb.jsonapi.exceptions.DocumentSerializationException;
import com.octopus.octopub.Constants;
import com.octopus.octopub.exceptions.InvalidInput;
import com.octopus.octopub.exceptions.MissingData;
import com.octopus.octopub.models.Audit;
import com.octopus.octopub.models.Product;
import com.octopus.octopub.repositories.AuditRepository;
import com.octopus.octopub.repositories.ProductRepository;
import java.nio.charset.StandardCharsets;
import java.util.List;
import javax.enterprise.context.ApplicationScoped;
import javax.inject.Inject;
import lombok.NonNull;

@ApplicationScoped
public class ProductsController {

  @Inject ProductRepository productRepository;

  @Inject AuditRepository auditRepository;

  @Inject ResourceConverter resourceConverter;

  @Inject TenantIdentifier tenantIdentifier;

  public String getAll(@NonNull final List<String> acceptHeaders, final String filterParam)
      throws DocumentSerializationException {
    final List<Product> products =
        productRepository.findAll(tenantIdentifier.getTenant(acceptHeaders), filterParam);
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

    product.tenant = tenantIdentifier.getTenant(acceptHeaders);
    productRepository.save(product);
    auditRepository.save(
        new Audit(
            Constants.MICROSERVICE_NAME,
            Constants.CREATED_ACTION,
            "Product-" + product.getId().toString()),
        acceptHeaders);

    return respondWithProduct(product);
  }

  public String update(@NonNull final String document, @NonNull final List<String> acceptHeaders)
      throws DocumentSerializationException {
    final Product product = getProductFromDocument(document);

    // The input was missing the required data
    if (product == null) {
      throw new MissingData();
    }

    // find the product we are updating
    final Product existingProduct = productRepository.findOne(product.id);
    // the existing product must have the same tenant asthe current request to be updated
    if (tenantIdentifier.getTenant(acceptHeaders).equals(existingProduct.tenant)) {
      // update the product details
      productRepository.update(product);

      // Create an audit record noting the change
      auditRepository.save(
          new Audit(
              Constants.MICROSERVICE_NAME,
              Constants.UPDATED_ACTION,
              "Product-" + product.getId().toString()),
          acceptHeaders);

      return respondWithProduct(product);
    } else {
      // Create an audit record noting the failure
      auditRepository.save(
          new Audit(
              Constants.MICROSERVICE_NAME,
              Constants.UPDATED_FAILED_TENANT_MISMATCH_ACTION,
              "Product-" + product.getId().toString()),
          acceptHeaders);
      // Throw an exception, which will be picked up by a Provider to create a custom response
      throw new InvalidInput("Failed to update a record created by another tenant.");
    }
  }

  public String getOne(@NonNull final String id, @NonNull final List<String> acceptHeaders)
      throws DocumentSerializationException {
    try {
      final Product product = productRepository.findOne(Integer.parseInt(id));
      if (product != null
          && (Constants.DEFAULT_TENANT.equals(product.getTenant())
              || tenantIdentifier.getTenant(acceptHeaders).equals(product.getTenant()))) {
        return respondWithProduct(product);
      }
    } catch (final NumberFormatException ex) {
      // ignored, as the supplied id was not an int, and would never find any entities
    }
    return null;
  }

  public boolean delete(@NonNull final String id, @NonNull final List<String> acceptHeaders)
      throws DocumentSerializationException {
    try {
      final Integer intId = Integer.parseInt(id);
      final Product product = productRepository.findOne(intId);
      if (product != null
          && (Constants.DEFAULT_TENANT.equals(product.getTenant())
              || tenantIdentifier.getTenant(acceptHeaders).equals(product.getTenant()))) {
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
    return productDocument.get();
  }

  private String respondWithProduct(@NonNull final Product product)
      throws DocumentSerializationException {
    final JSONAPIDocument<Product> document = new JSONAPIDocument<Product>(product);
    return new String(resourceConverter.writeDocument(document));
  }
}
