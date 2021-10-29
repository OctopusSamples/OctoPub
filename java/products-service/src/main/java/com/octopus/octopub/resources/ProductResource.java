package com.octopus.octopub.resources;

import com.octopus.octopub.models.Product;
import com.octopus.octopub.repositories.ProductRepository;
import javax.inject.Inject;
import javax.ws.rs.GET;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.core.Response;
import javax.ws.rs.core.Response.Status;
import nl.michelbijnen.jsonapi.parser.JsonApiConverter;

@Path("/api/products")
public class ProductResource {

  @Inject
  ProductRepository productRepository;

  @GET
  @Path("{id}")
  public Response get(@PathParam("id") final String id) {
    final Product product = productRepository.findOne(id);
    if (product != null) return Response.ok(JsonApiConverter.convert(product)).build();
    return Response.status(Status.NOT_FOUND).build();
  }
}
