// Package httphandler contains the logic required to respond to API requests
package httphandler

import (
	"github.com/google/jsonapi"
	"github.com/gorilla/mux"
	"github.com/mcasperson/OctoPub/go/votes-service/internal/pkg/models"
	"log"
	"net/http"
	"strconv"
	"time"
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
	intID, err := getID(r)
	if err != nil {
		writeError(w, err)
		return
	}

	jsonapiRuntime := buildRuntime("votes.show")

	// but, for now
	vote := &models.Vote{
		ID:         intID,
		CreatedAt:  time.Time{},
		IPAddress:  "",
		VoteObject: &models.Urn{ID: "urn:products:1"},
	}

	setJSONAPIContentType(w)

	writeModel(w, vote, jsonapiRuntime)
}

func listVotes(w http.ResponseWriter, r *http.Request) {
	jsonapiRuntime := buildRuntime("blogs.list")

	votes := []*models.Vote{{ID: 1, CreatedAt: time.Time{}, IPAddress: "", VoteObject: &models.Urn{ID: "urn:products:1"}}}

	setJSONAPIContentType(w)

	writeModel(w, votes, jsonapiRuntime)
}

func getID(r *http.Request) (int, error) {
	vars := mux.Vars(r)
	id := vars["id"]

	return strconv.Atoi(id)
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
