package com.octopus.octopub.configuration;

import io.quarkus.deployment.annotations.BuildProducer;
import io.quarkus.deployment.builditem.nativeimage.ServiceProviderBuildItem;
import io.quarkus.deployment.util.ServiceUtil;
import io.quarkus.logging.Log;
import io.quarkus.runtime.annotations.RegisterForReflection;
import io.quarkus.deployment.annotations.BuildStep;
import java.io.IOException;
import java.util.Set;

/** Ensure any classes reflected at runtime are bundled into the native build. */
@RegisterForReflection(targets = {io.jsonwebtoken.impl.DefaultJwtParserBuilder.class,
    io.jsonwebtoken.impl.compression.DefaultCompressionCodecResolver.class,
    io.jsonwebtoken.impl.compression.DeflateCompressionCodec.class,
    io.jsonwebtoken.impl.compression.GzipCompressionCodec.class})
public class ReflectionConfiguration {


}
