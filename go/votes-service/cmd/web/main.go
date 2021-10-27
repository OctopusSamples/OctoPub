package main

import (
	"github.com/mcasperson/OctoPub/go/votes-service/pkg/httphandler"
	"log"
	"net/http"
)

func main() {
	exampleHandler := &httphandler.HttpHandler{}
	http.HandleFunc("/votes", exampleHandler.ServeHTTP)
	log.Fatal(http.ListenAndServe(":8080", nil))
}
