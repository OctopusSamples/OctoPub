# OctoPub

OctoPub is an online library. 

It is written as microservices in a variety of languages and supports deployments to modern platforms like Lambdas and Kubernetes.

One of the prinicipals guiding this application is that it should be easy to deploy and test. So, by default, all services run with an in memory 
database, which removes the need to deploy specialized external databases for simple tests.

## Docker Compose

To run the application locally, execute the following command:

```bash
curl -fsS https://raw.githubusercontent.com/OctopusSamples/OctoPub/master/docker/docker-compose.yml | docker-compose -f - up -d
```
