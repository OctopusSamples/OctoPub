package com.octopus.octopub.config;

import com.github.jasminb.jsonapi.IntegerIdHandler;
import com.github.jasminb.jsonapi.StringIdHandler;
import com.octopus.octopub.models.Audit;
import com.octopus.octopub.models.Health;
import com.octopus.octopub.models.Product;
import io.quarkus.runtime.annotations.RegisterForReflection;

/**
 * This class is used to configure which other classes must be included in the native image intact.
 * Otherwise the native compilation will strip out unreferenced methods, which can cause issues with
 * reflection.
 */
@RegisterForReflection(
    targets = {StringIdHandler.class, Product.class, Health.class, Audit.class, IntegerIdHandler.class})
public class MyReflectionConfiguration {}
