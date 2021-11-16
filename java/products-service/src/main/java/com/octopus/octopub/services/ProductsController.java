package com.octopus.octopub.services;

import com.github.jasminb.jsonapi.JSONAPIDocument;
import com.github.jasminb.jsonapi.ResourceConverter;
import com.github.jasminb.jsonapi.exceptions.DocumentSerializationException;
import com.octopus.octopub.Constants;
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

  @Inject
  ProductRepository productRepository;

  @Inject
  AuditRepository auditRepository;

  @Inject
  ResourceConverter resourceConverter;

  @Inject
  TenantIdentifier tenantIdentifier;

  public String getAll(@NonNull final String acceptHeader)
      throws DocumentSerializationException {
    final List<Product> products = productRepository.findAll(
        tenantIdentifier.getTenant(acceptHeader));
    final JSONAPIDocument<List<Product>> document = new JSONAPIDocument<List<Product>>(products);
    final byte[] content = resourceConverter.writeDocumentCollection(document);
    return new String(content);
  }

  public String create(
      @NonNull final String document,
      @NonNull final String acceptHeader)
      throws DocumentSerializationException {
    final Product product = getProductFromDocument(document);

    if (product == null) {
      throw new MissingData();
    }

    product.tenant = tenantIdentifier.getTenant(acceptHeader);
    productRepository.save(product);
    auditRepository.save(new Audit(
        Constants.MICROSERVICE_NAME,
        Constants.CREATED_ACTION,
        product.getId().toString(),
        product.tenant));

    return respondWithProduct(product);
  }

  public String getOne(@NonNull final String id, @NonNull final String acceptHeader)
      throws DocumentSerializationException {
    try {
      final Product product = productRepository.findOne(Integer.parseInt(id));
      if (product != null &&
          (Constants.DEFAULT_TENANT.equals(product.getTenant()) ||
              tenantIdentifier.getTenant(acceptHeader).equals(product.getTenant()))) {
        return respondWithProduct(product);
      }
    } catch (final NumberFormatException ex) {
      // ignored, as the supplied id was not an int, and would never find any entities
    }
    return null;
  }

  public boolean delete(@NonNull final String id, @NonNull final String acceptHeader)
      throws DocumentSerializationException {
    try {
      final Integer intId = Integer.parseInt(id);
      final Product product = productRepository.findOne(intId);
      if (product != null &&
          (Constants.DEFAULT_TENANT.equals(product.getTenant()) ||
              tenantIdentifier.getTenant(acceptHeader).equals(product.getTenant()))) {
        productRepository.delete(intId);
        return true;
      }
    } catch (final NumberFormatException ex) {
      // ignored, as the supplied id was not an int, and would never find any entities
    }

    return false;
  }

  private Product getProductFromDocument(@NonNull final String document) {
    final JSONAPIDocument<Product> productDocument = resourceConverter
        .readDocument(document.getBytes(StandardCharsets.UTF_8), Product.class);
    return productDocument.get();
  }

  private String respondWithProduct(@NonNull final Product product)
      throws DocumentSerializationException {
    final JSONAPIDocument<Product> document = new JSONAPIDocument<Product>(product);
    return new String(resourceConverter.writeDocument(document));
  }
}
