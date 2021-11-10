package com.octopus.octopub.lambda;

import static javax.transaction.Transactional.TxType.NEVER;
import static javax.transaction.Transactional.TxType.NOT_SUPPORTED;

import com.amazonaws.services.lambda.runtime.Context;
import com.amazonaws.services.lambda.runtime.RequestHandler;
import java.sql.Connection;
import java.sql.SQLException;
import java.util.Map;
import javax.inject.Inject;
import javax.inject.Named;
import javax.persistence.EntityManager;
import javax.sql.DataSource;
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
  DataSource defaultDataSource;

  @Override
  public ProxyResponse handleRequest(final Map<String, Object> stringObjectMap,
      final Context context) {

      try {
        final Connection connection = defaultDataSource.getConnection();
        final Database database = DatabaseFactory.getInstance()
            .findCorrectDatabaseImplementation(new JdbcConnection(connection));
        final Liquibase liquibase = new Liquibase(
            "db/changeLog.xml",
            new ClassLoaderResourceAccessor(),
            database);
        liquibase.update(new Contexts(), new LabelExpression());
        return new ProxyResponse("200", "ok");
      } catch (final LiquibaseException | SQLException ex) {
        return new ProxyResponse("500", ex.toString());
      }

  }
}
