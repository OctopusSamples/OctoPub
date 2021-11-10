package com.octopus.octopub.config;

import com.github.jasminb.jsonapi.StringIdHandler;
import com.octopus.octopub.models.Audit;
import com.octopus.octopub.models.Product;
import io.quarkus.runtime.annotations.RegisterForReflection;

@RegisterForReflection(targets={ StringIdHandler.class, Product.class, Audit.class })
public class MyReflectionConfiguration {

}
