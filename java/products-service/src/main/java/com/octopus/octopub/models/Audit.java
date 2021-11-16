package com.octopus.octopub.models;

import com.github.jasminb.jsonapi.annotations.Id;
import com.github.jasminb.jsonapi.annotations.Type;
import lombok.Data;

@Type("audits")
@Data
public class Audit {

  @Id
  private String id;
  private String subject;
  private String action;
  private String object;

  public Audit(final String subject, final String action, final String object) {
    this.subject = subject;
    this.action = action;
    this.object = object;
  }
}
