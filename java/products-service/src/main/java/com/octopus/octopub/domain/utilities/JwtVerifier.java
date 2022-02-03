package com.octopus.octopub.domain.utilities;

/** An interface exposing methods used to verify a request contains the correct authorization. */
public interface JwtVerifier {

  /**
   * Confirms if the JWT contains the specified Cognito group.
   *
   * @param jwt The JWT passed with the request.
   * @param group The group to find in the JWT.
   * @return true if the group is found, and false otherwise.
   */
  boolean jwtContainsCognitoGroup(String jwt, String group);
}
