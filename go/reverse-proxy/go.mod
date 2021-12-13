module github.com/mcasperson/OctoPub/go/reverse-proxy

go 1.17

require (
	github.com/aws/aws-lambda-go v1.27.0
	github.com/awslabs/aws-lambda-go-api-proxy v0.11.0
)

// +heroku goVersion 1.17
// +heroku install ./cmd/web/...
