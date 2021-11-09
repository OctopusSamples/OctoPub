package com.octopus.octopub.models;

import com.github.jasminb.jsonapi.annotations.Type;
import javax.persistence.Entity;
import javax.persistence.GeneratedValue;
import javax.persistence.GenerationType;
import javax.persistence.Id;
import lombok.Data;

@Entity
@Data
@Type("products")
public class Product {

  @Id
  @com.github.jasminb.jsonapi.annotations.Id
  @GeneratedValue(strategy= GenerationType.IDENTITY)
  public Integer id;

  public String tenant;

  public String name;
}
