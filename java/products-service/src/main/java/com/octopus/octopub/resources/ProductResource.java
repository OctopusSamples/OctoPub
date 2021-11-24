package com.octopus.octopub.resources;

import com.github.jasminb.jsonapi.exceptions.DocumentSerializationException;
import com.octopus.octopub.Constants;
import com.octopus.octopub.handlers.ProductsHandler;
import java.util.List;
import java.util.Optional;
import javax.enterprise.context.RequestScoped;
import javax.inject.Inject;
import javax.transaction.Transactional;
import javax.ws.rs.DELETE;
import javax.ws.rs.GET;
import javax.ws.rs.HeaderParam;
import javax.ws.rs.PATCH;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.QueryParam;
import javax.ws.rs.core.Context;
import javax.ws.rs.core.Response;
import javax.ws.rs.core.Response.Status;
import javax.ws.rs.core.SecurityContext;
import lombok.NonNull;
import org.eclipse.microprofile.rest.client.inject.RegisterRestClient;

/**
 * WHen this app is run as a web server, this class defines the REST API endpoints.
 */
@Path("/api/products")
@RequestScoped
public class ProductResource {

  @Inject
  ProductsHandler productsController;

  @GET
  @Transactional
  public Response getAll(
      @Context final SecurityContext ctx,
      @HeaderParam(Constants.ACCEPT_HEADER) final List<String> acceptHeader,
      @QueryParam(Constants.FILTER_QUERY_PARAM) final String filter)
      throws DocumentSerializationException {
    return Response.ok(productsController.getAll(acceptHeader, filter)).build();
  }

  @POST
  @Transactional
  public Response create(
      @NonNull final String document,
      @HeaderParam(Constants.ACCEPT_HEADER) final List<String> acceptHeader)
      throws DocumentSerializationException {
    return Response.ok(productsController.create(document, acceptHeader)).build();
  }

  @PATCH
  @Path("{id}")
  @Transactional
  public Response update(
      @NonNull final String document,
      @PathParam("id") final String id,
      @HeaderParam(Constants.ACCEPT_HEADER) final List<String> acceptHeader)
      throws DocumentSerializationException {
    return Response.ok(productsController.update(id, document, acceptHeader)).build();
  }

  @DELETE
  @Path("{id}")
  @Transactional
  public Response delete(
      @PathParam("id") final String id,
      @HeaderParam(Constants.ACCEPT_HEADER) final List<String> acceptHeader) {
    return Response.noContent().build();
  }

  @GET
  @Path("{id}")
  @Transactional
  public Response getOne(
      @PathParam("id") final String id,
      @HeaderParam(Constants.ACCEPT_HEADER) final List<String> acceptHeader)
      throws DocumentSerializationException {
    return Optional.ofNullable(productsController.getOne(id, acceptHeader))
        .map(d -> Response.ok(d).build())
        .orElse(Response.status(Status.NOT_FOUND).build());
  }
}
