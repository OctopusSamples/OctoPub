package com.octopus.octopub.models;

import com.github.jasminb.jsonapi.annotations.Type;
import javax.persistence.Entity;
import javax.persistence.GeneratedValue;
import javax.persistence.GenerationType;
import javax.persistence.Id;
import lombok.Data;
import org.hibernate.envers.Audited;

@Entity(name = "product")
@Data
@Type("products")
public class Product {

  @Id
  @com.github.jasminb.jsonapi.annotations.Id
  @GeneratedValue(strategy= GenerationType.IDENTITY)
  public Integer id;

  @Audited
  public String tenant;

  @Audited
  public String name;
}
