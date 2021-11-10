package com.octopus.octopub.resources;

import com.github.jasminb.jsonapi.exceptions.DocumentSerializationException;
import com.octopus.octopub.Constants;
import com.octopus.octopub.services.ProductsController;
import java.util.Optional;
import javax.enterprise.context.RequestScoped;
import javax.inject.Inject;
import javax.transaction.Transactional;
import javax.ws.rs.GET;
import javax.ws.rs.HeaderParam;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.core.Context;
import javax.ws.rs.core.Response;
import javax.ws.rs.core.Response.Status;
import javax.ws.rs.core.SecurityContext;
import lombok.NonNull;

@Path("/api/products")
@RequestScoped
public class ProductResource {

  @Inject
  ProductsController productsController;

  @GET
  public Response getAll(
      @Context final SecurityContext ctx,
      @HeaderParam(Constants.ACCEPT_HEADER) final String acceptHeader)
      throws DocumentSerializationException {
    return Response.ok(productsController.getAll(acceptHeader)).build();
  }

  @POST
  @Transactional
  public Response create(
      @Context final SecurityContext ctx,
      @NonNull final String document,
      @HeaderParam(Constants.ACCEPT_HEADER) final String acceptHeader)
      throws DocumentSerializationException {
    return Response.ok(productsController.create(document, acceptHeader)).build();
  }

  @GET
  @Path("{id}")
  public Response getOne(
      @Context final SecurityContext ctx,
      @PathParam("id") final String id,
      @HeaderParam(Constants.ACCEPT_HEADER) final String acceptHeader)
      throws DocumentSerializationException {
    return Optional.ofNullable(productsController.getOne(id, acceptHeader))
        .map(d -> Response.ok(d).build())
        .orElse(Response.status(Status.NOT_FOUND).build());
  }
}
