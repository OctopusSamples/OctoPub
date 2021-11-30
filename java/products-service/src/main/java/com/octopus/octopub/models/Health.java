package com.octopus.octopub.models;

import com.github.jasminb.jsonapi.annotations.Type;
import lombok.Builder;
import lombok.Data;

@Data
@Builder
@Type("healths")
public class Health {
  private String path;
  private String method;
  private String status;
}
