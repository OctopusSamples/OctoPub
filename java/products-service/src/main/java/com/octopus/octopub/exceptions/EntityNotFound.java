package com.octopus.octopub.exceptions;

/**
 * The exception thrown when a requested entity can not be found (or will not be found due to
 * security or data partitioning rules).
 */
public class EntityNotFound extends RuntimeException {
  public EntityNotFound() {
    super();
  }

  public EntityNotFound(final Exception ex) {
    super(ex);
  }
}
