module github.com/mcasperson/OctoPub/go/reverse-proxy

go 1.17

require (
	github.com/aws/aws-lambda-go v1.27.0
	github.com/aws/aws-sdk-go v1.42.22
	github.com/awslabs/aws-lambda-go-api-proxy v0.11.0
	github.com/stretchr/testify v1.6.1
	github.com/vibrantbyte/go-antpath v1.1.1
)

require (
	github.com/davecgh/go-spew v1.1.1 // indirect
	github.com/jmespath/go-jmespath v0.4.0 // indirect
	github.com/pmezard/go-difflib v1.0.0 // indirect
	gopkg.in/yaml.v3 v3.0.0-20200615113413-eeeca48fe776 // indirect
)

// +heroku goVersion 1.17
// +heroku install ./cmd/web/...
