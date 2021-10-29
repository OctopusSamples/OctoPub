package com.octopus.octopub;

import io.crnk.rs.CrnkFeature;
import java.util.Collections;
import java.util.Set;
import javax.ws.rs.ApplicationPath;
import javax.ws.rs.core.Application;

@ApplicationPath("/")
public class MyApplication extends Application {

  @Override
  public Set<Object> getSingletons() {
    CrnkFeature crnkFeature = new CrnkFeature();
    return Collections.singleton(crnkFeature);
  }
}