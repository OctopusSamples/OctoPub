package com.octopus.octopub.services;

import com.octopus.octopub.Constants;
import java.util.List;
import java.util.stream.Stream;
import javax.enterprise.context.ApplicationScoped;
import org.apache.commons.lang3.StringUtils;

@ApplicationScoped
public class PartitionIdentifier {

  /**
   * The "Accept" header contains the version and partition information. For more information see the
   * discussion at https://github.com/json-api/json-api/issues/406.
   *
   * @param header The "Accept" header
   * @return The partition that the request is made under, defaulting to main.
   */
  public String getPartition(final List<String> header) {
    if (header == null || header.size() == 0 || header.stream().allMatch(StringUtils::isAllBlank)) {
      return Constants.DEFAULT_PARTITION;
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
        .filter(a -> a[0].trim().equals(Constants.ACCEPT_PARTITION_INFO))
        .map(a -> a[1].trim())
        .findFirst()
        .orElse(Constants.DEFAULT_PARTITION);
  }
}
