package com.octopus.octopub.models;

import com.github.jasminb.jsonapi.annotations.Type;
import lombok.Builder;
import lombok.Data;

/**
 * Represents a health check response.
 */
@Data
@Builder
@Type("healths")
public class Health {
  private String path;
  private String method;
  private String status;
}
