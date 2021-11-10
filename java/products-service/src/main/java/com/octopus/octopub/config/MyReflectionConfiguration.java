package com.octopus.octopub.config;

import com.github.jasminb.jsonapi.StringIdHandler;
import io.quarkus.runtime.annotations.RegisterForReflection;

@RegisterForReflection(targets={ StringIdHandler.class })
public class MyReflectionConfiguration {

}
