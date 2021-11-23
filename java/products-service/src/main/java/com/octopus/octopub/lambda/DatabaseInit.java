package com.octopus.octopub.lambda;

import com.amazonaws.services.lambda.runtime.Context;
import com.amazonaws.services.lambda.runtime.RequestHandler;
import com.octopus.octopub.services.LiquidbaseUpdater;
import java.sql.Connection;
import java.sql.SQLException;
import java.util.Map;
import javax.inject.Inject;
import javax.inject.Named;
import javax.sql.DataSource;
import liquibase.Contexts;
import liquibase.LabelExpression;
import liquibase.Liquibase;
import liquibase.database.Database;
import liquibase.database.DatabaseFactory;
import liquibase.database.jvm.JdbcConnection;
import liquibase.exception.LiquibaseException;
import liquibase.resource.ClassLoaderResourceAccessor;

@Named("DatabaseInit")
public class DatabaseInit implements RequestHandler<Map<String, Object>, ProxyResponse> {

  @Inject
  LiquidbaseUpdater liquidbaseUpdater;

  @Override
  public ProxyResponse handleRequest(final Map<String, Object> stringObjectMap,
      final Context context) {

      try {
        liquidbaseUpdater.update();
        return new ProxyResponse("200", "ok");
      } catch (final LiquibaseException | SQLException ex) {
        return new ProxyResponse("500", ex.toString());
      }

  }
}
