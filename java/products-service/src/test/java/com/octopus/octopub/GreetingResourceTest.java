package com.octopus.octopub;

import static io.restassured.RestAssured.given;

import com.github.jasminb.jsonapi.ResourceConverter;
import com.github.jasminb.jsonapi.exceptions.DocumentSerializationException;
import com.octopus.octopub.services.LiquidbaseUpdater;
import io.quarkus.test.junit.QuarkusTest;
import io.restassured.http.Header;
import io.restassured.http.Headers;
import java.sql.SQLException;
import javax.inject.Inject;
import liquibase.exception.LiquibaseException;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.TestInstance;

@QuarkusTest
@TestInstance(TestInstance.Lifecycle.PER_CLASS)
public class GreetingResourceTest extends BaseTest {

  @Inject LiquidbaseUpdater liquidbaseUpdater;

  @Inject ResourceConverter resourceConverter;

  @BeforeAll
  public void setup() throws SQLException, LiquibaseException {
    liquidbaseUpdater.update();
  }

  @Test
  public void testCreateAndGetProduct() throws DocumentSerializationException {
    given()
        .headers(
            new Headers(
                new Header("Accept", "application/vnd.api+json"),
                new Header("Accept", "application/vnd.api+json; dataPartition=main")))
        .when()
        .body(
            productToResourceDocument(resourceConverter, createProduct("testCreateAndGetProduct")))
        .post("/api/products")
        .then()
        .statusCode(200)
        .body(
            new LambdaMatcher(
                a -> getProductFromDocument(resourceConverter, a.toString()) != null,
                "Resource should be returned"));

    given()
        .headers(
            new Headers(
                new Header("Accept", "application/vnd.api+json"),
                new Header("Accept", "application/vnd.api+json; dataPartition=main")))
        .when()
        .get("/api/products")
        .then()
        .statusCode(200)
        .body(
            new LambdaMatcher(
                a ->
                    getProductsFromDocument(resourceConverter, a.toString()).stream()
                        .anyMatch(p -> "testCreateAndGetProduct".equals(p.getName())),
                "Resource should be returned"));
  }
}
