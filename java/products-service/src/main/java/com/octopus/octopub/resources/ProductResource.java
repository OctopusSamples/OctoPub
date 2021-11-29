package com.octopus.octopub.resources;

import com.github.jasminb.jsonapi.exceptions.DocumentSerializationException;
import com.octopus.octopub.Constants;
import com.octopus.octopub.exceptions.InvalidAcceptHeaders;
import com.octopus.octopub.handlers.ProductsHandler;
import java.util.Arrays;
import java.util.List;
import java.util.Objects;
import java.util.Optional;
import javax.enterprise.context.RequestScoped;
import javax.inject.Inject;
import javax.transaction.Transactional;
import javax.ws.rs.Consumes;
import javax.ws.rs.DELETE;
import javax.ws.rs.GET;
import javax.ws.rs.HeaderParam;
import javax.ws.rs.PATCH;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.QueryParam;
import javax.ws.rs.core.Response;
import javax.ws.rs.core.Response.Status;
import lombok.NonNull;

/** WHen this app is run as a web server, this class defines the REST API endpoints. */
@Path("/api/products")
@RequestScoped
public class ProductResource {

  @Inject ProductsHandler productsController;

  /**
   * The resource collection endpoint.
   *
   * @param acceptHeader The "Accept" headers.
   * @param filter The RSQ query string.
   * @return a HTTP response object.
   * @throws DocumentSerializationException Thrown if the entity could not be converted to a JSONAPI
   *     resource.
   */
  @GET
  @Produces(Constants.JSONAPI_CONTENT_TYPE)
  @Transactional
  public Response getAll(
      @HeaderParam(Constants.ACCEPT_HEADER) final List<String> acceptHeader,
      @QueryParam(Constants.FILTER_QUERY_PARAM) final String filter)
      throws DocumentSerializationException {
    checkAcceptHeader(acceptHeader);
    return Response.ok(productsController.getAll(acceptHeader, filter)).build();
  }

  /**
   * The resource creation endpoint.
   *
   * @param document The JSONAPI resource to create.
   * @param acceptHeader The "Accept" headers.
   * @return An HTTP response object with the created resource.
   * @throws DocumentSerializationException Thrown if the entity could not be converted to a JSONAPI
   *     resource.
   */
  @POST
  @Consumes(Constants.JSONAPI_CONTENT_TYPE)
  @Produces(Constants.JSONAPI_CONTENT_TYPE)
  @Transactional
  public Response create(
      @NonNull final String document,
      @HeaderParam(Constants.ACCEPT_HEADER) final List<String> acceptHeader)
      throws DocumentSerializationException {
    checkAcceptHeader(acceptHeader);
    return Response.ok(productsController.create(document, acceptHeader)).build();
  }

  /**
   * The resource update endpoint.
   *
   * @param document The JSONAPI resource with fields to update.
   * @param id The ID of the resource to update
   * @param acceptHeader The "Accept" headers.
   * @return An HTTP response object with the updated resource.
   * @throws DocumentSerializationException Thrown if the entity could not be converted to a JSONAPI
   *     resource.
   */
  @PATCH
  @Consumes(Constants.JSONAPI_CONTENT_TYPE)
  @Produces(Constants.JSONAPI_CONTENT_TYPE)
  @Path("{id}")
  @Transactional
  public Response update(
      @NonNull final String document,
      @PathParam("id") final String id,
      @HeaderParam(Constants.ACCEPT_HEADER) final List<String> acceptHeader)
      throws DocumentSerializationException {
    checkAcceptHeader(acceptHeader);
    return Response.ok(productsController.update(id, document, acceptHeader)).build();
  }

  /**
   * The delete endpoint.
   *
   * @param id The ID of the record to delete.
   * @param acceptHeader The "Accept" headers.
   * @return a HTTP response object.
   */
  @DELETE
  @Produces(Constants.JSONAPI_CONTENT_TYPE)
  @Path("{id}")
  @Transactional
  public Response delete(
      @PathParam("id") final String id,
      @HeaderParam(Constants.ACCEPT_HEADER) final List<String> acceptHeader) {
    checkAcceptHeader(acceptHeader);
    if (productsController.delete(id, acceptHeader)) {
      return Response.noContent().build();
    }
    return Response.status(Status.NOT_FOUND).build();
  }

  /**
   * The individual resource endpoint.
   *
   * @param id The ID of the resource to return.
   * @param acceptHeader The "Accept" headers.
   * @return An HTTP response object with the matching resource.
   * @throws DocumentSerializationException Thrown if the entity could not be converted to a JSONAPI
   *     resource.
   */
  @GET
  @Produces(Constants.JSONAPI_CONTENT_TYPE)
  @Path("{id}")
  @Transactional
  public Response getOne(
      @PathParam("id") final String id,
      @HeaderParam(Constants.ACCEPT_HEADER) final List<String> acceptHeader)
      throws DocumentSerializationException {
    checkAcceptHeader(acceptHeader);
    return Optional.ofNullable(productsController.getOne(id, acceptHeader))
        .map(d -> Response.ok(d).build())
        .orElse(Response.status(Status.NOT_FOUND).build());
  }

  private void checkAcceptHeader(final List<String> acceptHeader) {
    if (acceptHeader == null || acceptHeader.isEmpty()) {
      return;
    }

    final boolean allAcceptHeadersHaveMediaTypes =
        acceptHeader.stream()
            .filter(Objects::nonNull)
            .flatMap(h -> Arrays.stream(h.split(",")))
            .map(String::trim)
            .filter(h -> h.startsWith(Constants.JSONAPI_CONTENT_TYPE))
            .noneMatch(Constants.JSONAPI_CONTENT_TYPE::equals);

    if (allAcceptHeadersHaveMediaTypes) {
      throw new InvalidAcceptHeaders();
    }
  }
}
