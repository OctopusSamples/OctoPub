package com.octopus.octopub.configuration;

import io.quarkus.deployment.annotations.BuildProducer;
import io.quarkus.deployment.annotations.BuildStep;
import io.quarkus.deployment.builditem.nativeimage.ServiceProviderBuildItem;
import io.quarkus.deployment.util.ServiceUtil;
import io.quarkus.logging.Log;
import java.io.IOException;
import java.util.Set;

public final class MyExtProcessor {
  @BuildStep
  void registerNativeImageResources(final BuildProducer<ServiceProviderBuildItem> services)
      throws IOException {
    /*
      Taken from https://quarkus.io/guides/writing-extensions#service-files.
      We need to expose the JWT library services.
     */

    final String service = "META-INF/services/" + io.jsonwebtoken.CompressionCodec.class.getName();

    // find out all the implementation classes listed in the service files
    final Set<String> implementations =
        ServiceUtil.classNamesNamedIn(Thread.currentThread().getContextClassLoader(),
            service);

    Log.debug("Registering CompressionCodec implementations: " + implementations);

    // register every listed implementation class so they can be instantiated
    // in native-image at run-time
    services.produce(
        new ServiceProviderBuildItem(io.jsonwebtoken.CompressionCodec.class.getName(),
            implementations.toArray(new String[0])));
  }
}
