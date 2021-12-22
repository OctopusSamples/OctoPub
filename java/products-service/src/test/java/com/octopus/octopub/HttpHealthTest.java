package com.octopus.octopub;

import static io.restassured.RestAssured.given;

import com.github.jasminb.jsonapi.ResourceConverter;
import com.github.jasminb.jsonapi.exceptions.DocumentSerializationException;
import com.octopus.octopub.domain.services.LiquidbaseUpdater;
import io.quarkus.test.junit.QuarkusTest;
import io.restassured.response.ValidatableResponse;
import java.sql.SQLException;
import javax.inject.Inject;
import liquibase.exception.LiquibaseException;
import lombok.NonNull;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.TestInstance;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.ValueSource;

@QuarkusTest
@TestInstance(TestInstance.Lifecycle.PER_CLASS)
public class HttpHealthTest extends BaseTest {

  @Inject LiquidbaseUpdater liquidbaseUpdater;

  @BeforeAll
  public void setup() throws SQLException, LiquibaseException {
    liquidbaseUpdater.update();
  }

  @ParameterizedTest
  @ValueSource(
      strings = {
        "/health/products/GET",
        "/health/products/POST",
        "/health/products/x/GET",
        "/health/products/x/DELETE",
        "/health/products/x/PATCH"
      })
  public void testCreateAndGetProduct(@NonNull final String path) {
    given().when().get(path).then().statusCode(200);
  }
}
