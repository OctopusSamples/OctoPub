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
	r.HandleFunc("/votes", listVotes)
	r.HandleFunc("/votes/{id}", showVote)
	log.Fatal(http.ListenAndServe("localhost:8080", r))
}

func showVote(w http.ResponseWriter, r *http.Request) {
	jsonapiRuntime := buildRuntime("votes.show")

	vote, err := data.GetRepository().FindOne(getID(r))
	if err != nil {
		writeError(w, err)
		return
	}

	setJSONAPIContentType(w)
	writeModel(w, vote, jsonapiRuntime)
}

func listVotes(w http.ResponseWriter, r *http.Request) {
	jsonapiRuntime := buildRuntime("votes.list")

	votes, err := data.GetRepository().FindAll()
	if err != nil {
		writeError(w, err)
		return
	}

	setJSONAPIContentType(w)
	writeModel(w, votes, jsonapiRuntime)
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
		writeError(w, err)
	} else {
		w.WriteHeader(http.StatusOK)
	}
}

func writeError(w http.ResponseWriter, err error) {
	http.Error(w, err.Error(), http.StatusInternalServerError)
}
