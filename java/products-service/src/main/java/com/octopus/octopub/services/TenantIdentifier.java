package com.octopus.octopub.services;

import com.octopus.octopub.Constants;
import java.util.List;
import java.util.stream.Stream;
import javax.enterprise.context.ApplicationScoped;
import org.apache.commons.lang3.StringUtils;

@ApplicationScoped
public class TenantIdentifier {

  /**
   * The "Accept" header contains the version and tenant information. For more information see the
   * discussion at https://github.com/json-api/json-api/issues/406.
   *
   * <p>So we expect to see headers like: "Accept: application/vnd.api+json; version=1.0" indicates
   * that version 1.0 of the service should be used. "Accept: application/vnd.api+json;
   * tenant=mytest" indicates the default version should be used with the test tenant "mytest".
   * "Accept: application/vnd.api+json; version=1.0-mytest" indicates version 1.0-mytest of the
   * service should be used, which also implies that the test tenant "mytest" should be used.
   *
   * @param header The "Accept" header
   * @return The tenant that the request is made under, defaulting to main.
   */
  public String getTenant(final List<String> header) {
    if (header == null || header.size() == 0 || header.stream().allMatch(StringUtils::isAllBlank)) {
      return Constants.DEFAULT_TENANT;
    }

    return header.stream()
        // split on semi colons
        .flatMap(h -> Stream.of(h.split(";")))
        // remove any blank strings
        .filter(s -> !StringUtils.isAllBlank(s))
        // trim all strings
        .map(String::trim)
        // remove the json api content type
        .filter(s -> !s.equals(Constants.JSONAPI_CONTENT_TYPE))
        // split everything else on an equals
        .map(s -> s.split("="))
        // the split is expected to produce 2 strings
        .filter(a -> a.length == 2)
        .filter(a -> a[0].trim().equals(Constants.ACCEPT_TENANT_INFO))
        .map(a -> a[1].trim())
        .findFirst()
        .orElse(Constants.DEFAULT_TENANT);
  }
}
