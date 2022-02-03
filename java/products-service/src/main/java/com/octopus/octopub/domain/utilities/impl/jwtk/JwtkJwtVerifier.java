package com.octopus.octopub.domain.utilities.impl.jwtk;

import com.auth0.jwk.UrlJwkProvider;
import com.octopus.octopub.domain.utilities.JwtVerifier;
import io.jsonwebtoken.Claims;
import io.jsonwebtoken.Jws;
import io.jsonwebtoken.Jwts;
import java.util.List;
import java.util.Optional;
import javax.enterprise.context.ApplicationScoped;
import lombok.NonNull;
import org.apache.commons.lang3.ArrayUtils;
import org.apache.commons.lang3.StringUtils;
import org.eclipse.microprofile.config.inject.ConfigProperty;

/** An implementation of JwtVerifier using Jwtk. */
@ApplicationScoped
public class JwtkJwtVerifier implements JwtVerifier {

  private static final String COGNITO_GROUPS = "cognito:groups";

  @ConfigProperty(name = "cognito.pool")
  Optional<String> cognitoPool;

  @ConfigProperty(name = "cognito.region")
  Optional<String> cognitoRegion;

  @ConfigProperty(name = "cognito.disable-auth")
  boolean cognitoDisableAuth;

  /** {@inheritDoc} */
  @Override
  public boolean jwtContainsCognitoGroup(@NonNull final String jwt, @NonNull final String group) {
    if (cognitoDisableAuth
        || cognitoRegion.isEmpty()
        || cognitoPool.isEmpty()
        || StringUtils.isEmpty(cognitoPool.get())
        || StringUtils.isEmpty(cognitoRegion.get())) {
      return false;
    }

    final Jws<Claims> claims =
        Jwts.parserBuilder().setSigningKeyResolver(new JwkKeyResolver(
            new UrlJwkProvider(
                "https://cognito-idp."
                    + cognitoRegion.get().trim()
                    + ".amazonaws.com/"
                    + cognitoPool.get().trim())))
            .build()
            .parseClaimsJws(jwt);
    if (claims.getBody().containsKey(COGNITO_GROUPS)) {
      final List groups = claims.getBody().get(COGNITO_GROUPS, List.class);
      return groups.stream().anyMatch(g -> g.toString().equals(group));
    }

    return false;
  }
}
