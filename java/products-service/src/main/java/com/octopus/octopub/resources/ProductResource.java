package com.octopus.octopub.resources;

import com.github.jasminb.jsonapi.DeserializationFeature;
import com.github.jasminb.jsonapi.JSONAPIDocument;
import com.github.jasminb.jsonapi.ResourceConverter;
import com.github.jasminb.jsonapi.exceptions.DocumentSerializationException;
import com.octopus.octopub.Constants;
import com.octopus.octopub.models.Audit;
import com.octopus.octopub.models.Product;
import com.octopus.octopub.repositories.AuditRepository;
import com.octopus.octopub.repositories.ProductRepository;
import com.octopus.octopub.services.JsonApiConverter;
import java.nio.charset.StandardCharsets;
import java.util.List;
import javax.inject.Inject;
import javax.transaction.Transactional;
import javax.ws.rs.GET;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.core.Response;
import javax.ws.rs.core.Response.Status;
import lombok.NonNull;

@Path("/api/products")
public class ProductResource {

  @Inject
  ProductRepository productRepository;

  @Inject
  AuditRepository auditRepository;

  @Inject
  JsonApiConverter jsonApiConverter;

  @GET
  public Response getAll() throws DocumentSerializationException {
    final List<Product> products = productRepository.findAll();
    final JSONAPIDocument<List<Product>> document = new JSONAPIDocument<List<Product>>(products);
    final byte[] content = jsonApiConverter.buildResourceConverter()
        .writeDocumentCollection(document);
    return Response.ok(new String(content)).build();
  }

  @POST
  @Transactional
  public Response create(@NonNull final String product)
      throws Exception {
    final ResourceConverter resourceConverter = jsonApiConverter.buildResourceConverter();
    resourceConverter.disableDeserializationOption(DeserializationFeature.REQUIRE_RESOURCE_ID);
    final JSONAPIDocument<Product> productDocument = resourceConverter.readDocument(
        product.getBytes(StandardCharsets.UTF_8), Product.class);
    final Product productEntity = productDocument.get();

    if (productEntity == null) {
      throw new Exception("Document did not contain an entity");
    }

    productRepository.save(productEntity);
    auditRepository.save(new Audit(
        Constants.MICROSERVICE_NAME,
        Constants.CREATED_ACTION,
        productEntity.getId().toString()));
    final JSONAPIDocument<Product> document = new JSONAPIDocument<Product>(productEntity);
    return Response.ok(jsonApiConverter.buildResourceConverter().writeDocument(document)).build();
  }

  @GET
  @Path("{id}")
  public Response getOne(@PathParam("id") final String id) {
    try {
      final Product product = productRepository.findOne(Integer.parseInt(id));
      if (product != null) {
        final JSONAPIDocument<Product> document = new JSONAPIDocument<Product>(product);
        return Response.ok(jsonApiConverter.buildResourceConverter().writeDocument(document))
            .build();
      }
    } catch (final Exception ex) {
      // ignored, as the id was likely not an int
    }
    return Response.status(Status.NOT_FOUND).build();
  }
}
