package com.octopus.octopub;

import static io.restassured.RestAssured.given;

import com.github.jasminb.jsonapi.ResourceConverter;
import com.github.jasminb.jsonapi.exceptions.DocumentSerializationException;
import com.octopus.octopub.domain.entities.Product;
import com.octopus.octopub.infrastructure.utilities.LiquidbaseUpdater;
import io.quarkus.test.junit.QuarkusTest;
import io.restassured.http.Header;
import io.restassured.http.Headers;
import io.restassured.response.ValidatableResponse;
import java.sql.SQLException;
import java.util.Objects;
import javax.inject.Inject;
import liquibase.exception.LiquibaseException;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.TestInstance;

@QuarkusTest
@TestInstance(TestInstance.Lifecycle.PER_CLASS)
public class HttpApiTest extends BaseTest {

  @Inject LiquidbaseUpdater liquidbaseUpdater;

  @Inject ResourceConverter resourceConverter;

  @BeforeAll
  public void setup() throws SQLException, LiquibaseException {
    liquidbaseUpdater.update();
  }

  @Test
  public void testCreateAndGetProduct() throws DocumentSerializationException {
    final ValidatableResponse response =
        given()
            .accept("application/vnd.api+json,application/vnd.api+json; dataPartition=main")
            .contentType("application/vnd.api+json")
            .when()
            .body(
                productToResourceDocument(
                    resourceConverter, createProduct("testCreateAndGetProduct")))
            .post("/api/products")
            .then()
            .statusCode(200)
            .body(
                new LambdaMatcher(
                    a -> getProductFromDocument(resourceConverter, a.toString()) != null,
                    "Resource should be returned"));

    final Product created =
        getProductFromDocument(resourceConverter, response.extract().body().asString());

    given()
        .accept("application/vnd.api+json,application/vnd.api+json; dataPartition=main")
        .when()
        .get("/api/products")
        .then()
        .statusCode(200)
        .body(
            new LambdaMatcher(
                a ->
                    getProductsFromDocument(resourceConverter, a.toString()).stream()
                        .anyMatch(p -> Objects.equals(created.getId(), p.getId())),
                "Resource should be returned"));

    given()
        .accept("application/vnd.api+json,application/vnd.api+json; dataPartition=main")
        .when()
        .get("/api/products/" + created.getId())
        .then()
        .statusCode(200)
        .body(
            new LambdaMatcher(
                a ->
                    getProductFromDocument(resourceConverter, a.toString())
                        .getName()
                        .equals(created.getName()),
                "Resource should be returned"));
  }

  @Test
  public void testCreateAndDeleteProduct() throws DocumentSerializationException {
    final ValidatableResponse response =
        given()
            .accept("application/vnd.api+json,application/vnd.api+json; dataPartition=main")
            .contentType("application/vnd.api+json")
            .when()
            .body(
                productToResourceDocument(
                    resourceConverter, createProduct("testCreateAndDeleteProduct")))
            .post("/api/products")
            .then()
            .statusCode(200)
            .body(
                new LambdaMatcher(
                    a -> getProductFromDocument(resourceConverter, a.toString()) != null,
                    "Resource should be returned"));

    final Product created =
        getProductFromDocument(resourceConverter, response.extract().body().asString());

    given()
        .accept("application/vnd.api+json,application/vnd.api+json; dataPartition=main")
        .when()
        .delete("/api/products/" + created.getId())
        .then()
        .statusCode(204);

    given()
        .accept("application/vnd.api+json,application/vnd.api+json; dataPartition=main")
        .when()
        .get("/api/products/" + created.getId())
        .then()
        .statusCode(404);
  }

  @Test
  public void testCreateAndUpdateProduct() throws DocumentSerializationException {
    final ValidatableResponse response =
        given()
            .accept("application/vnd.api+json,application/vnd.api+json; dataPartition=main")
            .contentType("application/vnd.api+json")
            .when()
            .body(
                productToResourceDocument(
                    resourceConverter, createProduct("testCreateAndUpdateProduct")))
            .post("/api/products")
            .then()
            .statusCode(200)
            .body(
                new LambdaMatcher(
                    a -> getProductFromDocument(resourceConverter, a.toString()) != null,
                    "Resource should be returned"));

    final Product created =
        getProductFromDocument(resourceConverter, response.extract().body().asString());

    given()
        .accept("application/vnd.api+json,application/vnd.api+json; dataPartition=main")
        .contentType("application/vnd.api+json")
        .when()
        .body(
            productToResourceDocument(
                resourceConverter, createProduct("testCreateAndUpdateProductUpdated")))
        .patch("/api/products/" + created.getId())
        .then()
        .statusCode(200)
        .body(
            new LambdaMatcher(
                a ->
                    getProductFromDocument(resourceConverter, a.toString())
                        .getName()
                        .equals("testCreateAndUpdateProductUpdated"),
                "Resource should be returned"));

    given()
        .accept("application/vnd.api+json,application/vnd.api+json; dataPartition=main")
        .when()
        .get("/api/products/" + created.getId())
        .then()
        .statusCode(200)
        .body(
            new LambdaMatcher(
                a ->
                    getProductFromDocument(resourceConverter, a.toString())
                        .getName()
                        .equals("testCreateAndUpdateProductUpdated"),
                "Resource should be returned"));
  }

  @Test
  public void partitionFromHeader() throws DocumentSerializationException {
    final ValidatableResponse response =
        given()
            .accept("application/vnd.api+json,application/vnd.api+json; dataPartition=header")
            .contentType("application/vnd.api+json")
            .when()
            .body(
                productToResourceDocument(
                    resourceConverter, createProduct("testCreateAndGetProduct", "body")))
            .post("/api/products")
            .then()
            .statusCode(200)
            .body(
                new LambdaMatcher(
                    a ->
                        getProductFromDocument(resourceConverter, a.toString())
                            .getDataPartition()
                            .equals("header"),
                    "Resource partition should have come from header"));

    final Product created =
        getProductFromDocument(resourceConverter, response.extract().body().asString());

    given()
        .accept("application/vnd.api+json,application/vnd.api+json; dataPartition=header")
        .contentType("application/vnd.api+json")
        .when()
        .body(
            productToResourceDocument(
                resourceConverter, createProduct("testCreateAndUpdateProductUpdated", "body")))
        .patch("/api/products/" + created.getId())
        .then()
        .statusCode(200)
        .body(
            new LambdaMatcher(
                a ->
                    getProductFromDocument(resourceConverter, a.toString())
                        .getDataPartition()
                        .equals("header"),
                "Resource partition should not be updated"));
  }

  @Test
  public void failToUpdateResourceInDifferentPartition() throws DocumentSerializationException {
    final ValidatableResponse response =
        given()
            .accept("application/vnd.api+json,application/vnd.api+json; dataPartition=main")
            .contentType("application/vnd.api+json")
            .when()
            .body(
                productToResourceDocument(
                    resourceConverter, createProduct("testCreateAndGetProduct")))
            .post("/api/products")
            .then()
            .statusCode(200);

    final Product created =
        getProductFromDocument(resourceConverter, response.extract().body().asString());

    given()
        .headers(
            new Headers(
                new Header("Content-Type", "application/vnd.api+json"),
                new Header(
                    "Accept",
                    "application/vnd.api+json, application/vnd.api+json; dataPartition=testing")))
        .when()
        .body(
            productToResourceDocument(
                resourceConverter, createProduct("testCreateAndUpdateProductUpdated", "body")))
        .patch("/api/products/" + created.getId())
        .then()
        .statusCode(404);
  }

  @Test
  public void failToDeleteResourceInDifferentPartition() throws DocumentSerializationException {
    final ValidatableResponse response =
        given()
            .accept("application/vnd.api+json,application/vnd.api+json; dataPartition=main")
            .contentType("application/vnd.api+json")
            .when()
            .body(
                productToResourceDocument(
                    resourceConverter, createProduct("testCreateAndGetProduct")))
            .post("/api/products")
            .then()
            .statusCode(200);

    final Product created =
        getProductFromDocument(resourceConverter, response.extract().body().asString());

    given()
        .accept("application/vnd.api+json, application/vnd.api+json; dataPartition=testing")
        .when()
        .body(
            productToResourceDocument(
                resourceConverter, createProduct("testCreateAndUpdateProductUpdated", "body")))
        .delete("/api/products/" + created.getId())
        .then()
        .statusCode(404);
  }

  @Test
  public void failToGetResourceInDifferentPartition() throws DocumentSerializationException {
    final ValidatableResponse response =
        given()
            .accept("application/vnd.api+json,application/vnd.api+json; dataPartition=testing2")
            .contentType("application/vnd.api+json")
            .when()
            .body(
                productToResourceDocument(
                    resourceConverter, createProduct("testCreateAndGetProduct")))
            .post("/api/products")
            .then()
            .statusCode(200);

    final Product created =
        getProductFromDocument(resourceConverter, response.extract().body().asString());

    given()
        .accept("application/vnd.api+json, application/vnd.api+json; dataPartition=testing")
        .when()
        .get("/api/products/" + created.getId())
        .then()
        .statusCode(404);
  }

  @Test
  public void failWithMissingContentTypeForPost() throws DocumentSerializationException {
    final ValidatableResponse response =
        given()
            .accept("application/vnd.api+json,application/vnd.api+json; dataPartition=main")
            .when()
            .body(
                productToResourceDocument(
                    resourceConverter, createProduct("testCreateAndGetProduct")))
            .post("/api/products")
            .then()
            .statusCode(415);
  }

  @Test
  public void failWithoutPlainAcceptForPost() throws DocumentSerializationException {
    final ValidatableResponse response =
        given()
            .accept("application/vnd.api+json; dataPartition=main")
            .contentType("application/vnd.api+json")
            .when()
            .body(
                productToResourceDocument(
                    resourceConverter, createProduct("testCreateAndGetProduct")))
            .post("/api/products")
            .then()
            .statusCode(406);
  }

  @Test
  public void failWithoutPlainAcceptForPatch() throws DocumentSerializationException {
    final ValidatableResponse response =
        given()
            .accept("application/vnd.api+json; dataPartition=main")
            .contentType("application/vnd.api+json")
            .when()
            .body(
                productToResourceDocument(
                    resourceConverter, createProduct("testCreateAndGetProduct")))
            .patch("/api/products/1")
            .then()
            .statusCode(406);
  }

  @Test
  public void failWithoutPlainAcceptForDelete() {
    final ValidatableResponse response =
        given()
            .accept("application/vnd.api+json; dataPartition=main")
            .when()
            .delete("/api/products/1")
            .then()
            .statusCode(406);
  }

  @Test
  public void failWithoutPlainAcceptForGet() {
    final ValidatableResponse response =
        given()
            .accept("application/vnd.api+json; dataPartition=main")
            .when()
            .get("/api/products/1")
            .then()
            .statusCode(406);
  }

  @Test
  public void failWithoutPlainAcceptForGetAll() {
    final ValidatableResponse response =
        given()
            .accept("application/vnd.api+json; dataPartition=main")
            .when()
            .get("/api/products")
            .then()
            .statusCode(406);
  }

  @Test
  public void failWithMissingContentTypeForPatch() throws DocumentSerializationException {
    final ValidatableResponse response =
        given()
            .accept("application/vnd.api+json,application/vnd.api+json; dataPartition=testing2")
            .contentType("application/vnd.api+json")
            .when()
            .body(
                productToResourceDocument(
                    resourceConverter, createProduct("testCreateAndGetProduct")))
            .post("/api/products")
            .then()
            .statusCode(200);

    final Product created =
        getProductFromDocument(resourceConverter, response.extract().body().asString());

    given()
        .accept("application/vnd.api+json,application/vnd.api+json; dataPartition=main")
        .when()
        .body(
            productToResourceDocument(
                resourceConverter, createProduct("testCreateAndUpdateProductUpdated", "body")))
        .patch("/api/products/" + created.getId())
        .then()
        .statusCode(415);
  }

  @Test
  public void testFilterResults() throws DocumentSerializationException {
    final ValidatableResponse response =
        given()
            .accept("application/vnd.api+json,application/vnd.api+json; dataPartition=main")
            .contentType("application/vnd.api+json")
            .when()
            .body(
                productToResourceDocument(
                    resourceConverter, createProduct("testCreateAndGetProduct")))
            .post("/api/products")
            .then()
            .statusCode(200)
            .body(
                new LambdaMatcher(
                    a -> getProductFromDocument(resourceConverter, a.toString()) != null,
                    "Resource should be returned"));

    final Product created =
        getProductFromDocument(resourceConverter, response.extract().body().asString());

    given()
        .accept("application/vnd.api+json,application/vnd.api+json; dataPartition=main")
        .when()
        .queryParam("filter", "id==" + created.getId())
        .get("/api/products")
        .then()
        .statusCode(200)
        .body(
            new LambdaMatcher(
                a ->
                    getProductsFromDocument(resourceConverter, a.toString()).stream()
                        .anyMatch(p -> Objects.equals(created.getId(), p.getId())),
                "Resource should be returned"));

    given()
        .accept("application/vnd.api+json,application/vnd.api+json; dataPartition=main")
        .when()
        .queryParam("filter", "name==testCreateAndGetProduct")
        .get("/api/products")
        .then()
        .statusCode(200)
        .body(
            new LambdaMatcher(
                a ->
                    getProductsFromDocument(resourceConverter, a.toString()).stream()
                        .anyMatch(p -> Objects.equals(created.getId(), p.getId())),
                "Resource should be returned"));

    given()
        .accept("application/vnd.api+json,application/vnd.api+json; dataPartition=main")
        .when()
        .queryParam("filter", "name!=blah")
        .get("/api/products")
        .then()
        .statusCode(200)
        .body(
            new LambdaMatcher(
                a ->
                    getProductsFromDocument(resourceConverter, a.toString()).stream()
                        .anyMatch(p -> Objects.equals(created.getId(), p.getId())),
                "Resource should be returned"));

    given()
        .accept("application/vnd.api+json,application/vnd.api+json; dataPartition=main")
        .when()
        .queryParam("filter", "name==test*")
        .get("/api/products")
        .then()
        .statusCode(200)
        .body(
            new LambdaMatcher(
                a ->
                    getProductsFromDocument(resourceConverter, a.toString()).stream()
                        .anyMatch(p -> Objects.equals(created.getId(), p.getId())),
                "Resource should be returned"));

    given()
        .accept("application/vnd.api+json,application/vnd.api+json; dataPartition=main")
        .when()
        .queryParam("filter", "name=in=(testCreateAndGetProduct)")
        .get("/api/products")
        .then()
        .statusCode(200)
        .body(
            new LambdaMatcher(
                a ->
                    getProductsFromDocument(resourceConverter, a.toString()).stream()
                        .anyMatch(p -> Objects.equals(created.getId(), p.getId())),
                "Resource should be returned"));

    given()
        .accept("application/vnd.api+json,application/vnd.api+json; dataPartition=main")
        .when()
        .queryParam("filter", "id<" + (created.getId() + 1))
        .get("/api/products")
        .then()
        .statusCode(200)
        .body(
            new LambdaMatcher(
                a ->
                    getProductsFromDocument(resourceConverter, a.toString()).stream()
                        .anyMatch(p -> Objects.equals(created.getId(), p.getId())),
                "Resource should be returned"));
  }

  @Test
  public void testBadFilterResults() {
    given()
        .accept("application/vnd.api+json,application/vnd.api+json; dataPartition=main")
        .when()
        .queryParam("filter", "&^$*^%#$")
        .get("/api/products")
        .then()
        .statusCode(400);
  }

  @Test
  public void testCreateWithoutBody() throws DocumentSerializationException {
    final ValidatableResponse response =
        given()
            .accept("application/vnd.api+json,application/vnd.api+json; dataPartition=main")
            .contentType("application/vnd.api+json")
            .when()
            .body("{}")
            .post("/api/products")
            .then()
            .statusCode(400);
  }

  @Test
  public void testUpdateWithoutBody() throws DocumentSerializationException {
    final ValidatableResponse response =
        given()
            .accept("application/vnd.api+json,application/vnd.api+json; dataPartition=main")
            .contentType("application/vnd.api+json")
            .when()
            .body("{}")
            .patch("/api/products/1")
            .then()
            .statusCode(400);
  }
}
