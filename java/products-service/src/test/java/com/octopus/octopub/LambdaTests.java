package com.octopus.octopub;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

import com.amazonaws.services.lambda.runtime.Context;
import com.amazonaws.services.lambda.runtime.events.APIGatewayProxyRequestEvent;
import com.github.jasminb.jsonapi.ResourceConverter;
import com.github.jasminb.jsonapi.exceptions.DocumentSerializationException;
import com.octopus.octopub.lambda.ProductApi;
import com.octopus.octopub.lambda.ProxyResponse;
import com.octopus.octopub.models.Product;
import com.octopus.octopub.services.LiquidbaseUpdater;
import io.quarkus.test.junit.QuarkusTest;
import java.sql.SQLException;
import java.util.HashMap;
import java.util.List;
import javax.inject.Inject;
import liquibase.exception.LiquibaseException;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.TestInstance;
import org.mockito.Mockito;

@QuarkusTest
@TestInstance(TestInstance.Lifecycle.PER_CLASS)
public class LambdaTests extends BaseTest {

  @Inject
  ProductApi productApi;

  @Inject
  LiquidbaseUpdater liquidbaseUpdater;

  @Inject ResourceConverter resourceConverter;

  @BeforeAll
  public void setup() throws SQLException, LiquibaseException {
    liquidbaseUpdater.update();
  }

  @Test
  public void testLambdaCreateAndGet() throws DocumentSerializationException {
    final APIGatewayProxyRequestEvent apiGatewayProxyRequestEvent = new APIGatewayProxyRequestEvent();
    apiGatewayProxyRequestEvent.setHeaders(new HashMap<>(){{put("Accept", "application/vnd.api+json,application/vnd.api+json; dataPartition=main");}});
    apiGatewayProxyRequestEvent.setHttpMethod("POST");
    apiGatewayProxyRequestEvent.setPath("/api/products");
    apiGatewayProxyRequestEvent.setBody(productToResourceDocument(
        resourceConverter, createProduct("testCreateAndGetProduct")));
    final ProxyResponse postResponse = productApi.handleRequest(apiGatewayProxyRequestEvent, Mockito.mock(Context.class));
    final Product postEntity = getProductFromDocument(resourceConverter, postResponse.body);
    assertEquals("testCreateAndGetProduct", postEntity.getName());
    assertEquals("200", postResponse.statusCode);

    final APIGatewayProxyRequestEvent getApiGatewayProxyRequestEvent = new APIGatewayProxyRequestEvent();
    getApiGatewayProxyRequestEvent.setHeaders(new HashMap<>(){{put("Accept", "application/vnd.api+json,application/vnd.api+json; dataPartition=main");}});
    getApiGatewayProxyRequestEvent.setHttpMethod("GET");
    getApiGatewayProxyRequestEvent.setPath("/api/products/" + postEntity.getId());
    final ProxyResponse getResponse = productApi.handleRequest(getApiGatewayProxyRequestEvent, Mockito.mock(Context.class));
    final Product getEntity = getProductFromDocument(resourceConverter, getResponse.body);
    assertEquals(getEntity.getName(), postEntity.getName());
  }

  @Test
  public void testLambdaCreateAndUpdate() throws DocumentSerializationException {
    final APIGatewayProxyRequestEvent apiGatewayProxyRequestEvent = new APIGatewayProxyRequestEvent();
    apiGatewayProxyRequestEvent.setHeaders(new HashMap<>(){{put("Accept", "application/vnd.api+json,application/vnd.api+json; dataPartition=main");}});
    apiGatewayProxyRequestEvent.setHttpMethod("POST");
    apiGatewayProxyRequestEvent.setPath("/api/products");
    apiGatewayProxyRequestEvent.setBody(productToResourceDocument(
        resourceConverter, createProduct("testCreateAndGetProduct")));
    final ProxyResponse postResponse = productApi.handleRequest(apiGatewayProxyRequestEvent, Mockito.mock(Context.class));
    final Product postEntity = getProductFromDocument(resourceConverter, postResponse.body);
    assertEquals("testCreateAndGetProduct", postEntity.getName());
    assertEquals("200", postResponse.statusCode);

    final APIGatewayProxyRequestEvent patchApiGatewayProxyRequestEvent = new APIGatewayProxyRequestEvent();
    patchApiGatewayProxyRequestEvent.setHeaders(new HashMap<>(){{put("Accept", "application/vnd.api+json,application/vnd.api+json; dataPartition=main");}});
    patchApiGatewayProxyRequestEvent.setHttpMethod("PATCH");
    patchApiGatewayProxyRequestEvent.setPath("/api/products/" + postEntity.getId());
    patchApiGatewayProxyRequestEvent.setBody(productToResourceDocument(
        resourceConverter, createProduct("testCreateAndGetProductUpdated")));
    final ProxyResponse patchResponse = productApi.handleRequest(patchApiGatewayProxyRequestEvent, Mockito.mock(Context.class));
    final Product patchEntity = getProductFromDocument(resourceConverter, patchResponse.body);
    assertEquals("testCreateAndGetProductUpdated", patchEntity.getName());
  }

  @Test
  public void testLambdaCreateAndDelete() throws DocumentSerializationException {
    final APIGatewayProxyRequestEvent apiGatewayProxyRequestEvent = new APIGatewayProxyRequestEvent();
    apiGatewayProxyRequestEvent.setHeaders(new HashMap<>(){{put("Accept", "application/vnd.api+json,application/vnd.api+json; dataPartition=main");}});
    apiGatewayProxyRequestEvent.setHttpMethod("POST");
    apiGatewayProxyRequestEvent.setPath("/api/products");
    apiGatewayProxyRequestEvent.setBody(productToResourceDocument(
        resourceConverter, createProduct("testCreateAndGetProduct")));
    final ProxyResponse postResponse = productApi.handleRequest(apiGatewayProxyRequestEvent, Mockito.mock(Context.class));
    final Product postEntity = getProductFromDocument(resourceConverter, postResponse.body);
    assertEquals("testCreateAndGetProduct", postEntity.getName());
    assertEquals("200", postResponse.statusCode);

    final APIGatewayProxyRequestEvent deleteApiGatewayProxyRequestEvent = new APIGatewayProxyRequestEvent();
    deleteApiGatewayProxyRequestEvent.setHeaders(new HashMap<>(){{put("Accept", "application/vnd.api+json,application/vnd.api+json; dataPartition=main");}});
    deleteApiGatewayProxyRequestEvent.setHttpMethod("DELETE");
    deleteApiGatewayProxyRequestEvent.setPath("/api/products/" + postEntity.getId());
    final ProxyResponse deleteResponse = productApi.handleRequest(deleteApiGatewayProxyRequestEvent, Mockito.mock(Context.class));
    assertEquals("204", deleteResponse.statusCode);
  }

  @Test
  public void testMissingPath() {
    final APIGatewayProxyRequestEvent apiGatewayProxyRequestEvent = new APIGatewayProxyRequestEvent();
    apiGatewayProxyRequestEvent.setHeaders(new HashMap<>(){{put("Accept", "application/vnd.api+json,application/vnd.api+json; dataPartition=main");}});
    apiGatewayProxyRequestEvent.setHttpMethod("GET");
    apiGatewayProxyRequestEvent.setPath("/api/blah");
    final ProxyResponse postResponse = productApi.handleRequest(apiGatewayProxyRequestEvent, Mockito.mock(Context.class));
    assertEquals("404", postResponse.statusCode);
  }

  @Test
  public void testLambdaCreateAndGetFilter() throws DocumentSerializationException {
    final APIGatewayProxyRequestEvent apiGatewayProxyRequestEvent = new APIGatewayProxyRequestEvent();
    apiGatewayProxyRequestEvent.setHeaders(new HashMap<>(){{put("Accept", "application/vnd.api+json,application/vnd.api+json; dataPartition=main");}});
    apiGatewayProxyRequestEvent.setHttpMethod("POST");
    apiGatewayProxyRequestEvent.setPath("/api/products");
    apiGatewayProxyRequestEvent.setBody(productToResourceDocument(
        resourceConverter, createProduct("testCreateAndGetProduct")));
    final ProxyResponse postResponse = productApi.handleRequest(apiGatewayProxyRequestEvent, Mockito.mock(Context.class));
    final Product postEntity = getProductFromDocument(resourceConverter, postResponse.body);
    assertEquals("testCreateAndGetProduct", postEntity.getName());
    assertEquals("200", postResponse.statusCode);

    {
      final APIGatewayProxyRequestEvent getApiGatewayProxyRequestEvent =
          new APIGatewayProxyRequestEvent();
      getApiGatewayProxyRequestEvent.setHeaders(
          new HashMap<>() {
            {
              put(
                  "Accept",
                  "application/vnd.api+json,application/vnd.api+json; dataPartition=main");
            }
          });
      getApiGatewayProxyRequestEvent.setHttpMethod("GET");
      getApiGatewayProxyRequestEvent.setPath("/api/products");
      getApiGatewayProxyRequestEvent.setQueryStringParameters(
          new HashMap<>() {
            {
              put("filter", "id==" + postEntity.getId());
            }
          });
      final ProxyResponse getResponse =
          productApi.handleRequest(getApiGatewayProxyRequestEvent, Mockito.mock(Context.class));
      final List<Product> getEntities =
          getProductsFromDocument(resourceConverter, getResponse.body);
      assertTrue(getEntities.stream().anyMatch(p -> p.getName().equals(postEntity.getName())));
    }

    {
      final APIGatewayProxyRequestEvent getApiGatewayProxyRequestEvent =
          new APIGatewayProxyRequestEvent();
      getApiGatewayProxyRequestEvent.setHeaders(
          new HashMap<>() {
            {
              put(
                  "Accept",
                  "application/vnd.api+json,application/vnd.api+json; dataPartition=main");
            }
          });
      getApiGatewayProxyRequestEvent.setHttpMethod("GET");
      getApiGatewayProxyRequestEvent.setPath("/api/products");
      getApiGatewayProxyRequestEvent.setQueryStringParameters(
          new HashMap<>() {
            {
              put("filter", "name==doesnotexist");
            }
          });
      final ProxyResponse getResponse =
          productApi.handleRequest(getApiGatewayProxyRequestEvent, Mockito.mock(Context.class));
      final List<Product> getEntities =
          getProductsFromDocument(resourceConverter, getResponse.body);
      assertTrue(getEntities.isEmpty());
    }
  }

  @Test
  public void testLambdaBadFilter() throws DocumentSerializationException {

    final APIGatewayProxyRequestEvent getApiGatewayProxyRequestEvent =
        new APIGatewayProxyRequestEvent();
    getApiGatewayProxyRequestEvent.setHeaders(
        new HashMap<>() {
          {
            put("Accept", "application/vnd.api+json,application/vnd.api+json; dataPartition=main");
          }
        });
    getApiGatewayProxyRequestEvent.setHttpMethod("GET");
    getApiGatewayProxyRequestEvent.setPath("/api/products");
    getApiGatewayProxyRequestEvent.setQueryStringParameters(
        new HashMap<>() {
          {
            put("filter", "(*&^%$&*(^)");
          }
        });
    final ProxyResponse getResponse =
        productApi.handleRequest(getApiGatewayProxyRequestEvent, Mockito.mock(Context.class));
    assertEquals("400", getResponse.statusCode);
  }
}
