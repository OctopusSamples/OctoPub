package com.octopus.octopub.models;

import io.crnk.core.resource.annotations.JsonApiId;
import io.crnk.core.resource.annotations.JsonApiResource;
import javax.persistence.Entity;
import javax.persistence.Id;

@JsonApiResource(type = "products")
@Entity
public class Product {
  @JsonApiId
  @Id
  public String id;

  public String tenant;

  public String name;


}
