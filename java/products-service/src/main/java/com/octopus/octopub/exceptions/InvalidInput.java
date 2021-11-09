package com.octopus.octopub.exceptions;

public class InvalidInput extends RuntimeException {
  public InvalidInput(final Exception ex) {
    super(ex);
  }
}
