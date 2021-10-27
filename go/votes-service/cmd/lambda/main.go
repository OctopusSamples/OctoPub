package main

import (
	"github.com/aws/aws-lambda-go/lambda"
	"github.com/mcasperson/OctoPub/go/votes-service/pkg/lambdahanlder"
)

func main() {
	lambda.Start(lambdahanlder.HandleRequest)
}
