package com.octopus.octopub.lambda;

import com.amazonaws.services.lambda.runtime.Context;
import com.amazonaws.services.lambda.runtime.RequestHandler;
import java.util.Map;
import javax.inject.Inject;
import javax.inject.Named;
import javax.persistence.EntityManager;
import javax.transaction.Transactional;
import liquibase.Contexts;
import liquibase.LabelExpression;
import liquibase.Liquibase;
import liquibase.database.Database;
import liquibase.database.DatabaseFactory;
import liquibase.database.jvm.JdbcConnection;
import liquibase.exception.LiquibaseException;
import liquibase.resource.ClassLoaderResourceAccessor;
import org.hibernate.Session;

@Named("DatabaseInit")
public class DatabaseInit implements RequestHandler<Map<String, Object>, ProxyResponse> {

  @Inject
  EntityManager em;

  @Override
  @Transactional
  public ProxyResponse handleRequest(final Map<String, Object> stringObjectMap,
      final Context context) {

    final Exception[] exception = {null};
    final Session session = em.unwrap(Session.class);
    session.doWork(connection -> {
      try {
        final Database database = DatabaseFactory.getInstance()
            .findCorrectDatabaseImplementation(new JdbcConnection(connection));
        final Liquibase liquibase = new Liquibase(
            "db/changeLog.xml",
            new ClassLoaderResourceAccessor(),
            database);
        liquibase.update(new Contexts(), new LabelExpression());
      } catch (final LiquibaseException ex) {
        exception[0] = ex;
      }
    });

    return exception[0] == null
        ? new ProxyResponse("200", "ok")
        : new ProxyResponse("500", exception[0].toString());

  }
}
