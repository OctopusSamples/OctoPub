package com.octopus.octopub.exceptions;

public class ParsingException extends RuntimeException {
  public ParsingException(final Exception ex) {
    super(ex);
  }
}
