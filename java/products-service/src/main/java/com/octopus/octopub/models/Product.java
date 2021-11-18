package com.octopus.octopub.models;

import com.github.jasminb.jsonapi.IntegerIdHandler;
import com.github.jasminb.jsonapi.annotations.Type;
import javax.persistence.Entity;
import javax.persistence.GeneratedValue;
import javax.persistence.GenerationType;
import javax.persistence.Id;
import javax.persistence.Table;
import lombok.Data;
import org.hibernate.envers.Audited;

@Entity
@Data
@Table(name = "product")
@Type("products")
public class Product {

  @Id
  @com.github.jasminb.jsonapi.annotations.Id(IntegerIdHandler.class)
  @GeneratedValue(strategy= GenerationType.IDENTITY)
  public Integer id;

  @Audited
  public String tenant;

  @Audited
  public String name;

  @Audited
  public String pdf;

  @Audited
  public String epub;

  @Audited
  public String image;

  @Audited
  public String description;
}
