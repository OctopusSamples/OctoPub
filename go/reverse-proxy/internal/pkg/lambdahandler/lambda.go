// Package lambdahandler contains the AWS lambda entry point
package lambdahandler

import (
	"context"
	"encoding/json"
	"errors"
	"github.com/aws/aws-lambda-go/events"
	"github.com/aws/aws-sdk-go/aws"
	"github.com/aws/aws-sdk-go/aws/session"
	"github.com/aws/aws-sdk-go/service/lambda"
	"github.com/awslabs/aws-lambda-go-api-proxy/handlerfunc"
	"github.com/vibrantbyte/go-antpath/antpath"
	"net/http"
	"net/http/httputil"
	"net/url"
	"os"
	"strings"
)

var matcher = antpath.New()

// HandleRequest takes the incoming Lambda request and forwards it to the downstream service
// defined in the "Accept" headers.
func HandleRequest(ctx context.Context, req events.APIGatewayProxyRequest) (events.APIGatewayProxyResponse, error) {
	url, lambda, err := extractUpstreamService(&req)

	if err == nil {

		if url != nil {
			handler := func(w http.ResponseWriter, httpReq *http.Request) {
				httputil.NewSingleHostReverseProxy(url).ServeHTTP(w, httpReq)
			}

			adapter := handlerfunc.New(handler)
			resp, proxyErr := adapter.ProxyWithContext(context.Background(), req)

			if proxyErr != nil {
				return events.APIGatewayProxyResponse{}, proxyErr
			}

			return resp, nil
		}

		callLambda(lambda, req)
	}

	return events.APIGatewayProxyResponse{}, err

}

func callLambda(lambdaName string, req events.APIGatewayProxyRequest) (*lambda.InvokeOutput, error) {
	sess := session.Must(session.NewSessionWithOptions(session.Options{
		SharedConfigState: session.SharedConfigEnable,
	}))

	region := os.Getenv("AWS_REGION")
	client := lambda.New(sess, &aws.Config{Region: &region})
	payload, err := json.Marshal(req)

	if err != nil {
		return nil, err
	}

	return client.Invoke(&lambda.InvokeInput{FunctionName: aws.String(lambdaName), Payload: payload})
}

func extractUpstreamService(req *events.APIGatewayProxyRequest) (*url.URL, string, error) {
	acceptAll, err := getHeader(req.Headers, req.MultiValueHeaders, "Accept")

	if err != nil {
		return nil, "", errors.New("accept header is required")
	}

	acceptArr := strings.Split(acceptAll, ",")
	for _, element := range acceptArr {
		acceptComponents := strings.Split(element, ";")
		for _, acceptComponent := range acceptComponents {
			trimmedAcceptComponent := strings.TrimSpace(acceptComponent)
			if strings.Contains(trimmedAcceptComponent, "=") {
				versionComponents := strings.Split(trimmedAcceptComponent, "=")
				if len(versionComponents) == 2 && matcher.Match(versionComponents[0], "version["+req.Path+"]") {
					parsedUrl, err := url.Parse(versionComponents[1])

					// downstream service was not a url, so assume it is a lambda
					if err != nil {

						// the value can't be empty or blank
						if len(strings.TrimSpace(versionComponents[1])) > 0 {
							return nil, versionComponents[1], err
						}
					} else {
						return parsedUrl, "", nil
					}
				}
			}
		}
	}

	return nil, "", errors.New("failed to find downstream service")
}

func getHeader(singleHeaders map[string]string, multiHeaders map[string][]string, header string) (string, error) {
	for key, element := range singleHeaders {
		if strings.EqualFold(key, header) {
			return element, nil
		}
	}

	for key, element := range multiHeaders {
		if strings.EqualFold(key, header) {
			return strings.Join(element, ","), nil
		}
	}

	return "", errors.New("key was not found")
}
