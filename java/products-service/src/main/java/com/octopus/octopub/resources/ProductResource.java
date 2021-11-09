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
import com.sun.xml.bind.v2.runtime.reflect.opt.Const;
import java.nio.charset.StandardCharsets;
import java.util.Arrays;
import java.util.List;
import java.util.Optional;
import java.util.stream.Collectors;
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
import org.apache.commons.lang3.StringUtils;
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
  public Response getAll(
      @Context final SecurityContext ctx,
      @HeaderParam(Constants.ACCEPT_HEADER) final String acceptHeader)
      throws DocumentSerializationException {
    final List<Product> products = productRepository.findAll(getTenant(acceptHeader));
    final JSONAPIDocument<List<Product>> document = new JSONAPIDocument<List<Product>>(products);
    final byte[] content = jsonApiConverter.buildResourceConverter()
        .writeDocumentCollection(document);
    return Response.ok(new String(content)).build();
  }

  @POST
  @Transactional
  public Response create(
      @Context final SecurityContext ctx,
      @NonNull final String document,
      @HeaderParam(Constants.ACCEPT_HEADER) final String acceptHeader)
      throws DocumentSerializationException {
    final Product product = getProductFromDocument(document);

    if (product == null) {
      throw new MissingData();
    }

    product.tenant = getTenant(acceptHeader);
    productRepository.save(product);
    auditRepository.save(new Audit(
        Constants.MICROSERVICE_NAME,
        Constants.CREATED_ACTION,
        product.getId().toString(),
        product.tenant));

    return respondWithProduct(product);
  }

  @GET
  @Path("{id}")
  public Response getOne(
      @Context final SecurityContext ctx,
      @PathParam("id") final String id,
      @HeaderParam(Constants.ACCEPT_HEADER) final String acceptHeader)
      throws DocumentSerializationException {
    try {
      final Product product = productRepository.findOne(Integer.parseInt(id));
      if (product != null &&
          (Constants.DEFAULT_TENANT.equals(product.getTenant()) ||
              getTenant(acceptHeader).equals(product.getTenant()))) {
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

  /**
   * The "Accept" header contains the version and tenant information. For more information see the
   * discussion at https://github.com/json-api/json-api/issues/406.
   * <p>
   * So we expect to see headers like: "Accept: application/vnd.api+json; version=1.0" indicates
   * that version 1.0 of the service should be used. "Accept: application/vnd.api+json;
   * tenant=mytest" indicates the default version should be used with the test tenant "mytest".
   * "Accept: application/vnd.api+json; version=1.0-mytest" indicates version 1.0-mytest of the
   * service should be used, which also implies that the test tenant "mytest" should be used.
   *
   * @param header The "Accept" header
   * @return The tenant that the request is made under, defaulting to main.
   */
  private String getTenant(final String header) {
    if (StringUtils.isAllBlank(header)) {
      return Constants.DEFAULT_TENANT;
    }

    final List<String[]> acceptElements =
        // split on semi colons
        Arrays.stream(header.split(";"))
            // remove any blank strings
            .filter(s -> !StringUtils.isAllBlank(s))
            // trim all strings
            .map(String::trim)
            // remove the json api content type
            .filter(s -> !s.equals(Constants.JSONAPI_CONTENT_TYPE))
            // split everything else on an equals
            .map(s -> s.split("="))
            // the split is expected to produce 2 strings
            .filter(a -> a.length == 2)
            .collect(Collectors.toList());

    final Optional<String> tenant = acceptElements.stream()
        .filter(a -> a[0].trim().equals(Constants.ACCEPT_TENANT_INFO))
        .map(a -> a[1])
        .findFirst();

    // The tenant option takes priority.
    if (tenant.isPresent()) {
      return tenant.get();
    }

    final Optional<String> version = acceptElements.stream()
        .filter(a -> a[0].trim().equals(Constants.ACCEPT_VERSION_INFO))
        .map(a -> a[1])
        .filter(s -> s.contains("-"))
        .map(s -> s.substring(s.indexOf("-") + 1))
        .filter(s -> !StringUtils.isAllBlank(s))
        .findFirst();

    /*
     If the version contains a prerelease (i.e. anything after a dash), treat
     that as the tenant. This ensures feature branch deployments always save
     records under a tenant.
     */
    return version.orElse(Constants.DEFAULT_TENANT);
  }
}
