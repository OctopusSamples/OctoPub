// Package lambdahandler contains the AWS lambda entry point
package lambdahandler

import (
	"context"
	"encoding/json"
	"errors"
	"github.com/OctopusSamples/OctoPub/go/reverse-proxy/internal/pkg/utils"
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
	"strconv"
	"strings"
)

var matcher = antpath.New()

// HandleRequest takes the incoming Lambda request and forwards it to the downstream service
// defined in the "Accept" headers.
func HandleRequest(_ context.Context, req events.APIGatewayProxyRequest) (events.APIGatewayProxyResponse, error) {
	resp, err := processRequest(req)
	if err != nil {
		return events.APIGatewayProxyResponse{}, err
	}
	return *resp, nil
}

func processRequest(req events.APIGatewayProxyRequest) (*events.APIGatewayProxyResponse, error) {
	upstreamUrl, upstreamLambda, err := extractUpstreamService(req)

	if err == nil {

		if upstreamUrl != nil {
			return httpReverseProxy(upstreamUrl, req)
		}

		return callLambda(upstreamLambda, req)
	}

	return callLambda(os.Getenv("DEFAULT_LAMBDA"), req)
}

func httpReverseProxy(upstreamUrl *url.URL, req events.APIGatewayProxyRequest) (*events.APIGatewayProxyResponse, error) {
	handler := func(w http.ResponseWriter, httpReq *http.Request) {
		// The host header for the upstream requests must match the upstream server
		// https://github.com/golang/go/issues/28168
		httpReq.Host = upstreamUrl.Host
		proxy := httputil.NewSingleHostReverseProxy(upstreamUrl)
		proxy.ServeHTTP(w, httpReq)
	}

	adapter := handlerfunc.New(handler)
	resp, proxyErr := adapter.ProxyWithContext(context.Background(), req)

	if proxyErr != nil {
		return nil, proxyErr
	}

	return &resp, nil
}

func callLambda(lambdaName string, req events.APIGatewayProxyRequest) (*events.APIGatewayProxyResponse, error) {
	sess := session.Must(session.NewSessionWithOptions(session.Options{
		SharedConfigState: session.SharedConfigEnable,
	}))

	region := utils.GetEnv("AWS_REGION", "us-west-1")
	client := lambda.New(sess, &aws.Config{Region: &region})
	payload, err := json.Marshal(req)

	if err != nil {
		return nil, err
	}

	lambdaResponse, lambdaErr := client.Invoke(&lambda.InvokeInput{FunctionName: aws.String(lambdaName), Payload: payload})

	if lambdaErr != nil {
		return nil, lambdaErr
	}

	return convertLambdaProxyResponse(lambdaResponse)
}

func extractUpstreamService(req events.APIGatewayProxyRequest) (http *url.URL, lambda string, err error) {
	acceptAll, err := getHeader(req.Headers, req.MultiValueHeaders, "Accept")

	if err != nil {
		return nil, "", errors.New("accept header is required")
	}

	for _, acceptComponent := range getComponentsFromHeader(acceptAll) {
		path, method, destination, err := getRuleComponents(acceptComponent)
		if err == nil {
			if pathAndMethodIsMatch(path, method, req) {

				// for convenience, rules can reference the destinations of other paths, allowing
				// complex rule sets to be updated with a single destination
				pathDest, err := getDestinationPath(acceptAll, destination)

				if err == nil {
					destination = pathDest
				}

				url, err := getDestinationUrl(destination)

				if err == nil {
					return url, "", nil
				}

				lambda, err := getDestinationLambda(destination)

				if err == nil {
					return nil, lambda, nil
				}
			}
		}
	}

	return nil, "", errors.New("failed to find downstream service")
}

func getComponentsFromHeader(header string) []string {
	var returnArray []string
	headerArray := strings.Split(header, ",")
	for _, element := range headerArray {
		components := strings.Split(element, ";")
		returnArray = append(returnArray, components...)
	}

	return returnArray
}

func pathAndMethodIsMatch(path string, method string, req events.APIGatewayProxyRequest) bool {
	// The path is an ant matcher that must match the requested path
	pathIsMatch := matcher.Match(path, req.Path)
	// AThe http method must match the current request
	methodIsMatch := strings.EqualFold(method, req.HTTPMethod)

	return pathIsMatch && methodIsMatch
}

func getRuleComponents(acceptComponent string) (string, string, string, error) {
	ruleComponents := strings.Split(strings.TrimSpace(acceptComponent), "=")
	// ensure the component has an equals sign
	if len(ruleComponents) == 2 {
		if strings.HasPrefix(ruleComponents[0], "version[") && strings.HasSuffix(ruleComponents[0], "]") {
			strippedVersion := strings.TrimSuffix(strings.TrimPrefix(ruleComponents[0], "version["), "]")
			pathAndMethod := strings.Split(strippedVersion, ":")
			return pathAndMethod[0], pathAndMethod[1], ruleComponents[1], nil
		}
	}

	return "", "", "", errors.New("component was not a valid rule")
}

func getDestinationPath(acceptAll string, ruleDestination string) (string, error) {
	if strings.HasPrefix(ruleDestination, "path[") && strings.HasSuffix(ruleDestination, "]") {

		strippedDest := strings.TrimSuffix(strings.TrimPrefix(ruleDestination, "path["), "]")

		for _, acceptComponent := range getComponentsFromHeader(acceptAll) {
			path, method, destination, err := getRuleComponents(acceptComponent)
			if err == nil && path+":"+method == strippedDest {
				return destination, nil
			}
		}
	}

	return "", errors.New("destination was not a path, or did not find the path")
}

func getDestinationUrl(ruleDestination string) (*url.URL, error) {
	if strings.HasPrefix(ruleDestination, "url[") && strings.HasSuffix(ruleDestination, "]") {

		trimmedDestination := strings.TrimSuffix(strings.TrimPrefix(ruleDestination, "url["), "]")

		// See if the downstream service is a valid URL
		parsedUrl, err := url.Parse(trimmedDestination)

		// downstream service was not a url, so assume it is a lambda
		if err == nil && strings.HasPrefix(trimmedDestination, "http") {
			return parsedUrl, nil
		}
	}

	return nil, errors.New("destination was not a URL")
}

func getDestinationLambda(ruleDestination string) (string, error) {
	if strings.HasPrefix(ruleDestination, "lambda[") && strings.HasSuffix(ruleDestination, "]") {

		return strings.TrimSuffix(strings.TrimPrefix(ruleDestination, "lambda["), "]"), nil
	}

	return "", errors.New("destination was not a lambda")
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

func convertLambdaProxyResponse(lambdaResponse *lambda.InvokeOutput) (*events.APIGatewayProxyResponse, error) {
	var data LenientAPIGatewayProxyResponse
	jsonErr := json.Unmarshal(lambdaResponse.Payload, &data)

	if jsonErr != nil {
		var data2 events.APIGatewayProxyResponse
		jsonErr2 := json.Unmarshal(lambdaResponse.Payload, &data2)

		if jsonErr2 != nil {
			return nil, jsonErr2
		}

		return &data2, nil
	}

	apiGatewayProxyResponse, conErr := data.toAPIGatewayProxyResponse()

	if conErr != nil {
		return nil, conErr
	}

	return &apiGatewayProxyResponse, nil
}

type LenientAPIGatewayProxyResponse struct {
	StatusCode        string              `json:"statusCode"`
	Headers           map[string]string   `json:"headers"`
	MultiValueHeaders map[string][]string `json:"multiValueHeaders"`
	Body              string              `json:"body"`
	IsBase64Encoded   bool                `json:"isBase64Encoded,omitempty"`
}

func (r *LenientAPIGatewayProxyResponse) toAPIGatewayProxyResponse() (events.APIGatewayProxyResponse, error) {
	statusCode, err := strconv.Atoi(r.StatusCode)

	if err != nil {
		return events.APIGatewayProxyResponse{}, err
	}

	return events.APIGatewayProxyResponse{
		StatusCode:        statusCode,
		Headers:           r.Headers,
		MultiValueHeaders: r.MultiValueHeaders,
		Body:              r.Body,
		IsBase64Encoded:   r.IsBase64Encoded,
	}, nil
}
