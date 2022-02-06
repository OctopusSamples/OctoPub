package com.octopus.octopub.configuration;

import io.quarkus.runtime.annotations.RegisterForReflection;

@RegisterForReflection(targets = {io.jsonwebtoken.impl.DefaultJwtParserBuilder.class})
public class ReflectionConfiguration {

}
