# OctoPub

OctoPub is an online library. It is hosted at [octopus.pub](https://octopus.pub/).

It is written as microservices in a variety of languages and supports deployments to modern platforms like Lambdas and Kubernetes.

## Features


### In Memory Databases
By default, all services run with an in memory database, which removes the need to deploy specialized external databases for simple tests.

### Database Migrations
Where possible, databases are configured automatically with migration tools like [Liquidbase](https://www.liquibase.org/) or the [Entity Framework Migrations ](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli).

### JSONAPI
The [JSONAPI](https://jsonapi.org/) specification has been implemented for the public microservice APIs.

### Health Checking
Health checks present an interesting challenge with microservices. When each URL can potentially be backed by an individual microservice, how do you
check the health of the system?

Traditional approaches like [Spring Actuator](https://docs.spring.io/spring-boot/docs/current/reference/html/actuator.html) provide a single health
endpoint that is assumed to represent the health of the other endpoints served by the web application. This one-to-many approach for reporting
health doesn't scale to Functions-as-a-Service (FaaS) though, as there is no guarantee that multiple endpoints will be served by one single application.

To support health checks on FaaS platforms, OctoPub exposes a `/health` endpoint which mirrors the `/api` endpoint. Each path under `/api` has a matching
path in under `/health`, and the HTTP method used to access the API is then appended to the health endpoint.

So, to check the health of the service responding to GET requests to `/api/products`, you would perform a GET request to `/health/products/GET`. And to check the 
health of the service responding to POST requests to `/api/products`. you would perform a GET request to `/health/products/POST`.

Behind the scenes it makes sense for the same service that responds to GET requests to `/api/products` to also respond to GET requests to `/health/products/GET`. Having the a service report its own health removes many potential incorrect health reports, so this is the approach OctoPub has taken.

## Docker Compose

To run the application locally, execute the following command:

```bash
curl -fsS https://raw.githubusercontent.com/OctopusSamples/OctoPub/master/docker/docker-compose.yml | docker-compose -f - up -d
```

Then open [http://localhost:7080](http://localhost:7080).

## Error Codes
### AuditsService-Lambda-GeneralFailure
A catch all error for any exceptions thrown while processing a Lambda request.

### AuditsService-Lambda-DIFailure
This is likely reported when the dependency injection configuration was incorrect.

### AuditsService-Migration-GeneralFailure
A catch all error for any exceptions thrown while processing peforming a datbase migration.

### AuditsService-Migration-DIFailure
This is likely reported when the dependency injection configuration was incorrect.

### AuditsService-SQS-GeneralFailure
A catch all error for any exceptions thrown while processing a SQS request.
