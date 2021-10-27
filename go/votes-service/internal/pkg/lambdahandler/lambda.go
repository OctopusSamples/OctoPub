// Package lambdahandler contains the AWS lambda entry point
package lambdahandler

import (
	"context"
	"github.com/aws/aws-lambda-go/events"
)

func HandleRequest(ctx context.Context, req events.APIGatewayProxyRequest) (string, error) {
	return req.Body, nil
}
