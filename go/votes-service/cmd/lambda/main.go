package main

import (
	"github.com/aws/aws-lambda-go/lambda"
	"github.com/mcasperson/OctoPub/go/votes-service/internal/pkg/lambdahandler"
)

func main() {
	lambda.Start(lambdahandler.HandleRequest)
}
