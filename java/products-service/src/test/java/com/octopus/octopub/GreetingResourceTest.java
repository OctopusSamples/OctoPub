package com.octopus.octopub;

import com.octopus.octopub.models.Audit;
import com.octopus.octopub.repositories.AuditRepository;
import io.quarkus.test.junit.QuarkusTest;
import javax.inject.Inject;
import org.junit.jupiter.api.Test;

import static io.restassured.RestAssured.given;
import static org.hamcrest.CoreMatchers.is;

@QuarkusTest
public class GreetingResourceTest {

    @Inject
    AuditRepository auditRepository;

    @Test
    public void testHelloEndpoint() {
        auditRepository.save(new Audit(
            Constants.MICROSERVICE_NAME,
            Constants.CREATED_ACTION,
            "Test"));
    }

}