package com.octopus.octopub.repositories;

import com.amazonaws.services.sqs.AmazonSQS;
import com.amazonaws.services.sqs.AmazonSQSClientBuilder;
import com.amazonaws.services.sqs.model.MessageAttributeValue;
import com.amazonaws.services.sqs.model.SendMessageRequest;
import com.github.jasminb.jsonapi.JSONAPIDocument;
import com.google.common.collect.ImmutableMap;
import com.octopus.octopub.Constants;
import com.octopus.octopub.models.Audit;
import com.octopus.octopub.services.AuditService;
import com.octopus.octopub.producers.JsonApiConverter;
import javax.enterprise.context.ApplicationScoped;
import javax.inject.Inject;
import lombok.NonNull;
import org.eclipse.microprofile.config.inject.ConfigProperty;
import org.eclipse.microprofile.rest.client.inject.RestClient;

@ApplicationScoped
public class AuditRepository {

  @RestClient
  AuditService auditResource;

  @Inject
  JsonApiConverter jsonApiConverter;

  @ConfigProperty(name = "aws.audits.message-queue-url")
  String queueUrl;

  public void save(@NonNull final Audit audit) {
    try {
      final JSONAPIDocument<Audit> document = new JSONAPIDocument<Audit>(audit);

      final AmazonSQS sqs = AmazonSQSClientBuilder.defaultClient();
      final SendMessageRequest sendMsgRequest = new SendMessageRequest()
          .withQueueUrl(queueUrl)
          .withMessageBody(new String(jsonApiConverter.buildResourceConverter().writeDocument(document)))
          .withMessageAttributes( new ImmutableMap.Builder<String, MessageAttributeValue>()
              .put("action", new MessageAttributeValue().withStringValue("Create"))
              .put("entity", new MessageAttributeValue().withStringValue("Individual"))
              .build());
      sqs.sendMessage(sendMsgRequest);
    } catch (final Exception ex) {
      // need to do something here to allow sagas to revert themselves
    }
  }
}
