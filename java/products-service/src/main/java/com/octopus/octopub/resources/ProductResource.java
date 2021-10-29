package com.octopus.octopub.resources;

import com.octopus.octopub.models.Audit;
import com.octopus.octopub.models.Product;
import com.octopus.octopub.repositories.AuditRepository;
import com.octopus.octopub.repositories.ProductRepository;
import java.util.List;
import javax.inject.Inject;
import javax.ws.rs.GET;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.core.Response;
import javax.ws.rs.core.Response.Status;
import nl.michelbijnen.jsonapi.parser.JsonApiConverter;

@Path("/api/products")
public class ProductResource {

  @Inject
  ProductRepository productRepository;

  @Inject
  AuditRepository auditRepository;

  @GET
  public Response getAll() {
    final List<Product> product = productRepository.findAll();
    return Response.ok(JsonApiConverter.convert(product)).build();
  }

  @POST
  public Response create(final Product product) {
    productRepository.save(product);
    auditRepository.save(new Audit(Product.PRODUCT_URN_PREFIX, "CREATED", product.getJsonApiId()));
    return Response.ok(JsonApiConverter.convert(product)).build();
  }

  @GET
  @Path("{id}")
  public Response getOne(@PathParam("id") final String id) {
    final Product product = productRepository.findOne(id);
    if (product != null) return Response.ok(JsonApiConverter.convert(product)).build();
    return Response.status(Status.NOT_FOUND).build();
  }
}
