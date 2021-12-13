package lambdahandler_test

import (
	"context"
	"github.com/OctopusSamples/OctoPub/go/reverse-proxy/internal/pkg/lambdahandler"
	"github.com/aws/aws-lambda-go/events"
	"github.com/stretchr/testify/assert"
	"testing"
)

func TestHandler(t *testing.T) {

	tests := []struct {
		request events.APIGatewayProxyRequest
		expect  string
		err     error
	}{
		{
			request: events.APIGatewayProxyRequest{
				Headers: map[string]string{
					"Accept":       "application/vnd.api+json,application/vnd.api+json; version[/api/products*]=Development-product-0",
					"Service-Name": "test",
				},
				Path:       "/api/products",
				HTTPMethod: "GET",
			},
			expect: "data",
			err:    nil,
		},
		{
			request: events.APIGatewayProxyRequest{
				Body: "My Request",
				Headers: map[string]string{
					"Accept":       "application/vnd.api+json,application/vnd.api+json; version[/post*]=https://postman-echo.com",
					"Service-Name": "test",
				},
				Path:       "/post",
				HTTPMethod: "POST",
			},
			expect: "My Request",
			err:    nil,
		},
		{
			request: events.APIGatewayProxyRequest{
				Body: "My Request",
				Headers: map[string]string{
					"Accept":       "application/vnd.api+json,application/vnd.api+json; version[/put*]=https://postman-echo.com",
					"Service-Name": "test",
				},
				Path:       "/put",
				HTTPMethod: "PUT",
			},
			expect: "My Request",
			err:    nil,
		},
		{
			request: events.APIGatewayProxyRequest{
				Headers: map[string]string{
					"Accept":       "application/vnd.api+json,application/vnd.api+json; version[/get*]=https://postman-echo.com",
					"Service-Name": "test",
				},
				Path:       "/get",
				HTTPMethod: "GET",
			},
			expect: "https://postman-echo.com/",
			err:    nil,
		},
	}

	for _, test := range tests {
		ctx := context.Background()
		response, err := lambdahandler.HandleRequest(ctx, test.request)
		assert.IsType(t, test.err, err)
		assert.Contains(t, response.Body, test.expect)
	}

}
