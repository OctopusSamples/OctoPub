package com.octopus.octopub.domain.utilities.impl.jwtk;

import com.auth0.jwk.Jwk;
import com.auth0.jwk.JwkException;
import com.auth0.jwk.JwkProvider;
import io.jsonwebtoken.Claims;
import io.jsonwebtoken.JwsHeader;
import io.jsonwebtoken.SigningKeyResolver;
import java.security.Key;
import lombok.NonNull;

/** A key resolved used to lookup keys from a JWK. */
public class JwkKeyResolver implements SigningKeyResolver {
  private final JwkProvider keyStore;

  /**
   * Constructor.
   *
   * @param keyStore The keystore used to access the keys.
   */
  public JwkKeyResolver(@NonNull final JwkProvider keyStore) {
    this.keyStore = keyStore;
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public Key resolveSigningKey(@NonNull final JwsHeader jwsHeader, final Claims claims) {
    return resolveSigningKey(jwsHeader);
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public Key resolveSigningKey(@NonNull final JwsHeader jwsHeader, final String plaintext) {
    return resolveSigningKey(jwsHeader);
  }

  private Key resolveSigningKey(@NonNull final JwsHeader jwsHeader) {
    try {
      final String keyId = jwsHeader.getKeyId();
      final Jwk pub = keyStore.get(keyId);
      return pub.getPublicKey();
    } catch (JwkException e) {
      return null;
    }
  }
}
