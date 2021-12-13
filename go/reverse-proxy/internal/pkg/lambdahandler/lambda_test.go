package lambdahandler_test

import (
	"context"
	"github.com/mcasperson/OctoPub/go/reverse-proxy/internal/pkg/lambdahandler"
	"testing"

	"github.com/aws/aws-lambda-go/events"
	"github.com/stretchr/testify/assert"
)

func TestHandler(t *testing.T) {

	tests := []struct {
		request events.APIGatewayProxyRequest
		expect  string
		err     error
	}{
		{
			request: events.APIGatewayProxyRequest{
				Body: "My Request",
				Headers: map[string]string{
					"Accept":       "application/vnd.api+json,application/vnd.api+json; test-version=https://postman-echo.com/post",
					"Service-Name": "test",
				},
				Path:       "/",
				HTTPMethod: "POST",
			},
			expect: "My Request",
			err:    nil,
		},
		{
			request: events.APIGatewayProxyRequest{
				Body: "My Request",
				Headers: map[string]string{
					"Accept":       "application/vnd.api+json,application/vnd.api+json; test-version=https://postman-echo.com/put",
					"Service-Name": "test",
				},
				Path:       "/",
				HTTPMethod: "PUT",
			},
			expect: "My Request",
			err:    nil,
		},
		{
			request: events.APIGatewayProxyRequest{
				Headers: map[string]string{
					"Accept":       "application/vnd.api+json,application/vnd.api+json; test-version=https://postman-echo.com/get",
					"Service-Name": "test",
				},
				Path:       "/",
				HTTPMethod: "GET",
			},
			expect: "https://postman-echo.com/get/",
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
