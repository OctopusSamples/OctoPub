package com.octopus.octopub.exceptions;

public class MissingData extends RuntimeException {
  public MissingData() {
    super();
  }

  public MissingData(final Exception ex) {
    super(ex);
  }
}
