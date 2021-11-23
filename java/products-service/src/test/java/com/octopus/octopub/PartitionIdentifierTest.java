package com.octopus.octopub;

import com.octopus.octopub.services.PartitionIdentifier;
import java.util.ArrayList;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.CsvSource;
import org.locationtech.jts.util.Assert;

public class PartitionIdentifierTest {
  public static final PartitionIdentifier PARTITION_IDENTIFIER = new PartitionIdentifier();

  @ParameterizedTest
  @CsvSource({
      "main,application/vnd.api+json; partition=main",
      "main,application/vnd.api+json; partition=main ",
      "main,application/vnd.api+json; partition= main ",
      "testing,application/vnd.api+json; partition=testing",
      "testing,application/vnd.api+json; partition=testing ",
      "testing,application/vnd.api+json; partition= testing ",
      "main,application/vnd.api+json; partition=",
      "main,application/vnd.api+json; partition= ",
      "main,application/vnd.api+json; ",
      "main,application/vnd.api+json"})
  public void testPartitions(final String expected, final String acceptHeader) {
    Assert.equals(expected, PARTITION_IDENTIFIER.getPartition(new ArrayList<>(){{add(acceptHeader);}}));
    Assert.equals(expected, PARTITION_IDENTIFIER.getPartition(new ArrayList<>(){{add(acceptHeader); add("application/vnd.api+json");}}));
  }
}