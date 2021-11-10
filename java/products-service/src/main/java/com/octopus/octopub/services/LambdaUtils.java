package com.octopus.octopub.services;

import java.util.Map;
import java.util.Optional;
import javax.enterprise.context.ApplicationScoped;
import lombok.NonNull;

@ApplicationScoped
public class LambdaUtils {

  public String getHeader(final Map<String, Object> stringObjectMap, final String header) {
    return Optional.ofNullable(getMap(stringObjectMap, "headers"))
        .map(m -> getString(m, header))
        .orElse("");
  }

  private String getString(@NonNull final Map<String, Object> stringObjectMap,
      @NonNull final String key) {
    return stringObjectMap.entrySet().stream()
        .filter(e -> e.getKey().toLowerCase().equals(key.toLowerCase()))
        .map(e -> e.getValue().toString())
        .findFirst()
        .orElse(null);
  }

  private Map<String, Object> getMap(@NonNull final Map<String, Object> stringObjectMap,
      @NonNull final String key) {
    if (!stringObjectMap.containsKey(key)) {
      return null;
    }
    return (Map<String, Object>) stringObjectMap.get(key);
  }
}
