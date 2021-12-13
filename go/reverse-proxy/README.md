This project implements a simple reverse proxy Lambda capable of forwarding requests to a downstream HTTP service
or Lambda.

The intention of this proxy is to allow Lambda requests to be routed to feature branch instances. The instances can be
hosted on a developers local machine and exposed to public traffic with services like [ngrok](https://ngrok.com/) or via
a more direct tunnel such as a VPN. Alternatively, feature branch Lambdas can be deployed alongside their mainline
siblings.

Redirection rules are defined in the `Accept` header based on ant wildcard paths. For example, the header 
`version[/api/products*]=https://c9ce-118-208-2-185.ngrok.io` instructs this proxy to redirect all requests made on
paths that match `/api/products*` to https://c9ce-118-208-2-185.ngrok.io. A header like
`version[/api/products*]=Development-products-0-myfeature` will redirect requests made on
paths that match `/api/products*` to the Lambda called `Development-products-0-myfeature`.

This allows a client to make a request to a top level API with `Accept` headers like 
`version[/api/products*]=https://c9ce-118-208-2-185.ngrok.io;version[/api/audits*]=Development-audits-0-myfeature`,
and so long as each service forwards the `Accept` header to each service it calls, feature branch instances of 
deeply nested microservices will be executed without having to recreate the entire microservice ecosystem locally.