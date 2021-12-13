package com.octopus.octopub.models;

import com.github.jasminb.jsonapi.StringIdHandler;
import com.github.jasminb.jsonapi.annotations.Id;
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
  @Id
  public String endpoint;

  public String path;
  public String method;
  public String status;
}
