package com.octopus.octopub.config;

import com.github.jasminb.jsonapi.StringIdHandler;
import com.github.jasminb.jsonapi.annotations.Id;
import io.quarkus.runtime.annotations.RegisterForReflection;

@RegisterForReflection(targets={ StringIdHandler.class, Id.class })
public class MyReflectionConfiguration {

}
