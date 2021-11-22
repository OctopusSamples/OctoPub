package com.octopus.octopub.exceptions;

public class EntityNotFound extends RuntimeException {
  public EntityNotFound() {
    super();
  }

  public EntityNotFound(final Exception ex) {
    super(ex);
  }
}
