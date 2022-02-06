package com.octopus.octopub.configuration;

import io.quarkus.runtime.annotations.RegisterForReflection;

/** Ensure any classes reflected at runtime are bundled into the native build. */
@RegisterForReflection(targets = {io.jsonwebtoken.impl.DefaultJwtParserBuilder.class,
    io.jsonwebtoken.impl.compression.DefaultCompressionCodecResolver.class,
    io.jsonwebtoken.impl.compression.DeflateCompressionCodec.class,
    io.jsonwebtoken.impl.compression.GzipCompressionCodec.class})
public class ReflectionConfiguration {}
