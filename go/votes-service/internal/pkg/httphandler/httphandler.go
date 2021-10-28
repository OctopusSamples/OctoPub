// Package httphandler contains the logic required to respond to API requests
package httphandler

import (
	"github.com/google/jsonapi"
	"github.com/gorilla/mux"
	"github.com/mcasperson/OctoPub/go/votes-service/internal/pkg/data"
	"log"
	"net/http"
)

const (
	headerAccept      = "Accept"
	headerContentType = "Content-Type"
)

func StartServer() {
	r := mux.NewRouter()
	r.HandleFunc("/api/votes", listVotes)
	r.HandleFunc("/api/votes/{id}", showVote)

	r.HandleFunc("/health/votes", healthOk)
	r.HandleFunc("/health/votes/{id}", healthOk)
	log.Fatal(http.ListenAndServe("localhost:8080", r))
}

func healthOk(w http.ResponseWriter, r *http.Request) {
	w.WriteHeader(http.StatusOK)
}

func showVote(w http.ResponseWriter, r *http.Request) {
	if !processAcceptHeader(r) {
		writeAcceptError(w)
		return
	}

	jsonapiRuntime := buildRuntime("votes.show")

	vote, err := data.GetRepository().FindOne(getID(r), GetTenant(r))
	if err != nil {
		writeServerError(w, err)
		return
	}

	if vote == nil {
		http.NotFound(w, r)
		return
	}

	setJSONAPIContentType(w)
	writeModel(w, vote, jsonapiRuntime)
}

func listVotes(w http.ResponseWriter, r *http.Request) {
	if !processAcceptHeader(r) {
		writeAcceptError(w)
		return
	}

	jsonapiRuntime := buildRuntime("votes.list")

	votes, err := data.GetRepository().FindAll(GetTenant(r))
	if err != nil {
		writeServerError(w, err)
		return
	}

	setJSONAPIContentType(w)
	writeModel(w, votes, jsonapiRuntime)
}

func processAcceptHeader(r *http.Request) bool {
	for _, h := range r.Header.Values(headerAccept) {
		if h == jsonapi.MediaType {
			return true
		}
	}

	return false
}

func getID(r *http.Request) string {
	return mux.Vars(r)["id"]
}

func buildRuntime(key string) *jsonapi.Runtime {
	return jsonapi.NewRuntime().Instrument(key)
}

func setJSONAPIContentType(w http.ResponseWriter) {
	w.Header().Set(headerContentType, jsonapi.MediaType)
}

func writeModel(w http.ResponseWriter, model interface{}, jsonapiRuntime *jsonapi.Runtime) {
	if err := jsonapiRuntime.MarshalPayload(w, model); err != nil {
		writeServerError(w, err)
	} else {
		w.WriteHeader(http.StatusOK)
	}
}

func writeServerError(w http.ResponseWriter, err error) {
	http.Error(w, err.Error(), http.StatusInternalServerError)
}

func writeAcceptError(w http.ResponseWriter) {
	http.Error(w, "415 Unsupported Media Type", http.StatusUnsupportedMediaType)
}
