package com.octopus.octopub.models;

import com.github.jasminb.jsonapi.annotations.Id;
import com.github.jasminb.jsonapi.annotations.Type;
import lombok.Data;

@Type("audits")
@Data
public class Audit {

  @Id
  public String id;
  public String tenant;
  public String subject;
  public String action;
  public String object;

  public Audit(final String subject, final String action, final String object) {
    this.subject = subject;
    this.action = action;
    this.object = object;
  }
}
