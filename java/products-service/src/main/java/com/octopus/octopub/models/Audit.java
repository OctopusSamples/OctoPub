package com.octopus.octopub.models;

public class Audit {
  public String id;
  public String tenant;
  public String subject;
  public String action;
  public String object;

  public Audit(String subject, String action, String object) {
    this.subject = subject;
    this.action = action;
    this.object = object;
  }
}
