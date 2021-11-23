package com.octopus.octopub.services;

import java.sql.Connection;
import java.sql.SQLException;
import javax.enterprise.context.ApplicationScoped;
import javax.inject.Inject;
import javax.sql.DataSource;
import liquibase.Contexts;
import liquibase.LabelExpression;
import liquibase.Liquibase;
import liquibase.database.Database;
import liquibase.database.DatabaseFactory;
import liquibase.database.jvm.JdbcConnection;
import liquibase.exception.LiquibaseException;
import liquibase.resource.ClassLoaderResourceAccessor;

@ApplicationScoped
public class LiquidbaseUpdater {
  @Inject
  DataSource defaultDataSource;

  public void update() throws SQLException, LiquibaseException {
    final Connection connection = defaultDataSource.getConnection();
    final Database database = DatabaseFactory.getInstance()
        .findCorrectDatabaseImplementation(new JdbcConnection(connection));
    final Liquibase liquibase = new Liquibase(
        "db/changeLog.xml",
        new ClassLoaderResourceAccessor(),
        database);
    liquibase.update(new Contexts(), new LabelExpression());
  }
}
