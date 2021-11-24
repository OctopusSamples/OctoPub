package com.octopus.octopub;

import com.octopus.octopub.models.Audit;
import com.octopus.octopub.repositories.AuditRepository;
import io.quarkus.test.junit.QuarkusTest;
import java.util.List;
import javax.inject.Inject;
import org.junit.jupiter.api.Test;

@QuarkusTest
public class GreetingResourceTest {

  @Inject AuditRepository auditRepository;

  public void testHelloEndpoint() {
    auditRepository.save(
        new Audit(Constants.MICROSERVICE_NAME, Constants.CREATED_ACTION, "Test"),
        List.of(Constants.JSON_CONTENT_TYPE + "; dataPartition=integration-tests"));
  }
}
