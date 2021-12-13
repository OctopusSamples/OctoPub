// Package lambdahandler contains the AWS lambda entry point
package lambdahandler

import (
	"context"
	"errors"
	"github.com/aws/aws-lambda-go/events"
	"github.com/awslabs/aws-lambda-go-api-proxy/handlerfunc"
	"net/http"
	"net/http/httputil"
	"net/url"
	"strings"
)

// HandleRequest takes the incoming Lambda request and forwards it to the downstream service
// defined in the "Accept" headers.
func HandleRequest(ctx context.Context, req events.APIGatewayProxyRequest) (events.APIGatewayProxyResponse, error) {
	handler := func(w http.ResponseWriter, req *http.Request) {
		url, err := extractUpstreamService(req)

		if err != nil {
			w.WriteHeader(400)
			w.Write([]byte(err.Error()))
			return
		}

		httputil.NewSingleHostReverseProxy(url).ServeHTTP(w, req)
	}

	adapter := handlerfunc.New(handler)
	resp, err := adapter.ProxyWithContext(context.Background(), req)

	if err != nil {
		return events.APIGatewayProxyResponse{}, err
	}

	return resp, nil
}

func extractUpstreamService(req *http.Request) (*url.URL, error) {
	serviceName := req.Header.Get("Service-Name")

	if serviceName == "" {
		return nil, errors.New("service-name header is required")
	}

	acceptAll := req.Header.Get("Accept")

	if acceptAll == "" {
		return nil, errors.New("accept header is required")
	}

	acceptArr := strings.Split(acceptAll, ",")
	for _, element := range acceptArr {
		acceptComponents := strings.Split(element, ";")
		for _, acceptComponent := range acceptComponents {
			trimmedAcceptComponent := strings.TrimSpace(acceptComponent)
			if strings.HasPrefix(trimmedAcceptComponent, serviceName+"-version=") {
				versionComponents := strings.Split(element, "=")
				url, err := url.Parse(versionComponents[1])

				if err != nil {
					return nil, err
				}

				return url, nil
			}
		}
	}

	return nil, errors.New("failed to find downstream service")
}
