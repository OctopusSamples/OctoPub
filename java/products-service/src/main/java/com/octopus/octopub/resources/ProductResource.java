package com.octopus.octopub.resources;

import com.github.jasminb.jsonapi.JSONAPIDocument;
import com.github.jasminb.jsonapi.exceptions.DocumentSerializationException;
import com.octopus.octopub.Constants;
import com.octopus.octopub.exceptions.MissingData;
import com.octopus.octopub.models.Audit;
import com.octopus.octopub.models.Product;
import com.octopus.octopub.repositories.AuditRepository;
import com.octopus.octopub.repositories.ProductRepository;
import com.octopus.octopub.services.JsonApiConverter;
import java.nio.charset.StandardCharsets;
import java.util.List;
import javax.enterprise.context.RequestScoped;
import javax.inject.Inject;
import javax.transaction.Transactional;
import javax.ws.rs.GET;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.core.Context;
import javax.ws.rs.core.Response;
import javax.ws.rs.core.Response.Status;
import javax.ws.rs.core.SecurityContext;
import lombok.NonNull;
import org.eclipse.microprofile.jwt.JsonWebToken;

@Path("/api/products")
@RequestScoped
public class ProductResource {

  @Inject
  ProductRepository productRepository;

  @Inject
  AuditRepository auditRepository;

  @Inject
  JsonApiConverter jsonApiConverter;

  @Inject
  JsonWebToken jwt;

  @GET
  public Response getAll(@Context SecurityContext ctx) throws DocumentSerializationException {
    final List<Product> products = productRepository.findAll();
    final JSONAPIDocument<List<Product>> document = new JSONAPIDocument<List<Product>>(products);
    final byte[] content = jsonApiConverter.buildResourceConverter()
        .writeDocumentCollection(document);
    return Response.ok(new String(content)).build();
  }

  @POST
  @Transactional
  public Response create(@Context SecurityContext ctx, @NonNull final String document)
      throws DocumentSerializationException {
    final Product product = getProductFromDocument(document);

    if (product == null) {
      throw new MissingData();
    }

    productRepository.save(product);
    auditRepository.save(new Audit(
        Constants.MICROSERVICE_NAME,
        Constants.CREATED_ACTION,
        product.getId().toString()));

    return respondWithProduct(product);
  }

  @GET
  @Path("{id}")
  public Response getOne(@Context SecurityContext ctx, @PathParam("id") final String id)
      throws DocumentSerializationException {
    try {
      final Product product = productRepository.findOne(Integer.parseInt(id));
      if (product != null) {
        return respondWithProduct(product);
      }
    } catch (final NumberFormatException ex) {
      // ignored, as the supplied id was not an int, and would never find any entities
    }
    return Response.status(Status.NOT_FOUND).build();
  }

  private Product getProductFromDocument(@NonNull final String document) {
    final JSONAPIDocument<Product> productDocument = jsonApiConverter.buildResourceConverter()
        .readDocument(document.getBytes(StandardCharsets.UTF_8), Product.class);
    return productDocument.get();
  }

  private Response respondWithProduct(@NonNull final Product product)
      throws DocumentSerializationException {
    final JSONAPIDocument<Product> document = new JSONAPIDocument<Product>(product);
    return Response.ok(jsonApiConverter.buildResourceConverter().writeDocument(document)).build();
  }
}
