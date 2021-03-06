package com.octopus.octopub.domain.handlers;

import com.github.jasminb.jsonapi.JSONAPIDocument;
import com.github.jasminb.jsonapi.ResourceConverter;
import com.github.jasminb.jsonapi.exceptions.DocumentSerializationException;
import com.octopus.octopub.GlobalConstants;
import com.octopus.octopub.domain.Constants;
import com.octopus.octopub.domain.entities.Audit;
import com.octopus.octopub.domain.entities.Product;
import com.octopus.octopub.domain.exceptions.EntityNotFound;
import com.octopus.octopub.domain.exceptions.Unauthorized;
import com.octopus.octopub.domain.utilities.JwtUtils;
import com.octopus.octopub.domain.utilities.JwtVerifier;
import com.octopus.octopub.domain.utilities.PartitionIdentifier;
import com.octopus.octopub.domain.utilities.impl.JoseJwtVerifier;
import com.octopus.octopub.infrastructure.repositories.AuditRepository;
import com.octopus.octopub.infrastructure.repositories.ProductRepository;
import java.nio.charset.StandardCharsets;
import java.util.List;
import javax.enterprise.context.ApplicationScoped;
import javax.inject.Inject;
import lombok.NonNull;
import org.eclipse.microprofile.config.inject.ConfigProperty;

/**
 * Handlers take the raw input from the upstream service, like Lambda or a web server, convert the
 * inputs to POJOs, apply the security rules, create an audit trail, and then pass the requests down
 * to repositories.
 */
@ApplicationScoped
public class ProductsHandler {

  @ConfigProperty(name = "cognito.editor-group")
  String cognitoEditorGroup;

  @ConfigProperty(name = "cognito.disable-auth")
  Boolean cognitoDisableAuth;

  @Inject
  ProductRepository productRepository;

  @Inject
  AuditRepository auditRepository;

  @Inject
  ResourceConverter resourceConverter;

  @Inject
  PartitionIdentifier partitionIdentifier;

  @Inject
  JoseJwtVerifier jwtVerifier;

  @Inject
  JwtUtils jwtUtils;

  /**
   * Returns all matching resources.
   *
   * @param acceptHeaders The "accept" headers.
   * @param filterParam The filter query param.
   * @return All matching resources
   * @throws DocumentSerializationException Thrown if the entity could not be converted to a JSONAPI resource.
   */
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

  /**
   * Creates a new resource.
   *
   * @param document The JSONAPI resource to create.
   * @param acceptHeaders The "accept" headers.
   * @return The newly created resource
   * @throws DocumentSerializationException Thrown if the entity could not be converted to a JSONAPI resource.
   */
  public String create(
      @NonNull final String document,
      @NonNull final List<String> acceptHeaders,
      final String authorizationHeader)
      throws DocumentSerializationException {

    if (!isAuthorized(authorizationHeader)) {
      throw new Unauthorized();
    }

    final Product product = getProductFromDocument(document);

    product.dataPartition = partitionIdentifier.getPartition(acceptHeaders);

    productRepository.save(product);
    auditRepository.save(
        new Audit(
            GlobalConstants.MICROSERVICE_NAME,
            Constants.CREATED_ACTION,
            "Product-" + product.getId().toString()),
        acceptHeaders);

    return respondWithProduct(product);
  }

  /**
   * Updates an existing resource.
   *
   * @param id The ID of the resource to update.
   * @param document The JSONAPI resource document with the updates.
   * @param acceptHeaders The "accept" headers.
   * @return The updated resource
   * @throws DocumentSerializationException Thrown if the entity could not be converted to a JSONAPI resource.
   */
  public String update(
      @NonNull final String id,
      @NonNull final String document,
      @NonNull final List<String> acceptHeaders)
      throws DocumentSerializationException {
    final Product product = getProductFromDocument(document);

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
              new Audit(GlobalConstants.MICROSERVICE_NAME, Constants.UPDATED_ACTION, "Product-" + intId),
              acceptHeaders);

          return respondWithProduct(updated);

        } else {
          // Create an audit record noting the failure
          auditRepository.save(
              new Audit(
                  GlobalConstants.MICROSERVICE_NAME,
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

  /**
   * Returns the one resource that matches the supplied ID.
   *
   * @param id The ID of the resource to return.
   * @param acceptHeaders The "accept" headers.
   * @return The matching resource.
   * @throws DocumentSerializationException Thrown if the entity could not be converted to a JSONAPI resource.
   */
  public String getOne(@NonNull final String id, @NonNull final List<String> acceptHeaders)
      throws DocumentSerializationException {
    try {
      final Product product = productRepository.findOne(Integer.parseInt(id));
      if (product != null
          && (Constants.DEFAULT_PARTITION.equals(product.getDataPartition())
              || partitionIdentifier
                  .getPartition(acceptHeaders)
                  .equals(product.getDataPartition()))) {
        return respondWithProduct(product);
      }
    } catch (final NumberFormatException ex) {
      // ignored, as the supplied id was not an int, and would never find any entities
    }
    throw new EntityNotFound();
  }

  /**
   * Deletes the selected resource.
   *
   * @param id The ID of the resource to delete.
   * @param acceptHeaders The "accept" headers.
   * @return true if the resource was deleted, and false if it was not found.
   */
  public boolean delete(@NonNull final String id, @NonNull final List<String> acceptHeaders) {
    try {
      final Integer intId = Integer.parseInt(id);
      final Product product = productRepository.findOne(intId);
      // The product being deleted must match the current partition
      if (product != null
          && partitionIdentifier.getPartition(acceptHeaders).equals(product.getDataPartition())) {
        productRepository.delete(intId);
        auditRepository.save(
            new Audit(GlobalConstants.MICROSERVICE_NAME, Constants.DELETED_ACTION, "Product-" + intId),
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

  private boolean isAuthorized(final String authorizationHeader) {
    return cognitoDisableAuth || jwtUtils.getJwtFromAuthorizationHeader(authorizationHeader)
        .map(jwt -> jwtVerifier.jwtContainsCognitoGroup(jwt, cognitoEditorGroup))
        .orElse(false);
  }

}
