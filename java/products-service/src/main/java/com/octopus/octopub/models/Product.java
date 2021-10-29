package com.octopus.octopub.models;

import javax.persistence.Entity;
import javax.persistence.GeneratedValue;
import javax.persistence.GenerationType;
import javax.persistence.Id;
import javax.persistence.Transient;
import nl.michelbijnen.jsonapi.annotation.JsonApiId;
import nl.michelbijnen.jsonapi.annotation.JsonApiObject;

@JsonApiObject("products")
@Entity
public class Product {

  public static String PRODUCT_URN_PREFIX = "urn:products";

  @Id
  @GeneratedValue(strategy= GenerationType.IDENTITY)
  public int id;

  @JsonApiId
  @Transient
  public String jsonApiId;

  public String tenant;

  public String name;

  public String getJsonApiId() {
      return PRODUCT_URN_PREFIX + ":" + id;
  }

  public void setJsonApiId(final String value) {
    // ignored
  }
}
