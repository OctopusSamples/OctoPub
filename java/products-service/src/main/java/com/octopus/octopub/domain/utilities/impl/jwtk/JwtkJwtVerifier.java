package com.octopus.octopub.domain.utilities.impl.jwtk;

import com.auth0.jwk.UrlJwkProvider;
import com.octopus.octopub.domain.utilities.JwtVerifier;
import io.jsonwebtoken.Claims;
import io.jsonwebtoken.Jws;
import io.jsonwebtoken.Jwts;
import javax.enterprise.context.ApplicationScoped;
import javax.inject.Inject;
import lombok.NonNull;
import org.apache.commons.lang3.ArrayUtils;
import org.eclipse.microprofile.config.inject.ConfigProperty;

/**
 * An implementation of JwtVerifier using Jwtk.
 */
@ApplicationScoped
public class JwtkJwtVerifier implements JwtVerifier {

  private static final String COGNITO_GROUPS = "cognito:groups";

  @Inject
  @ConfigProperty(name = "cognito.pool")
  String cognitoPool;

  @Inject
  @ConfigProperty(name = "cognito.region")
  String cognitoRegion;

  private final JwkKeyResolver jwkKeyResolver;

  /**
   * Constructor.
   */
  public JwtkJwtVerifier() {
    this.jwkKeyResolver =
        new JwkKeyResolver(
            new UrlJwkProvider(
                "https://cognito-idp."
                    + cognitoRegion
                    + ".amazonaws.com/"
                    + cognitoPool
                    + "/.well-known/jwks.json"));
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public boolean jwtContainsCognitoGroup(@NonNull final String jwt, @NonNull final String group) {
    final Jws<Claims> claims =
        Jwts.parserBuilder().setSigningKeyResolver(jwkKeyResolver).build().parseClaimsJws(jwt);
    if (claims.getBody().containsKey(COGNITO_GROUPS)) {
      final String[] groups = claims.getBody().get(COGNITO_GROUPS, String[].class);
      return ArrayUtils.contains(groups, group);
    }

    return false;
  }
}
