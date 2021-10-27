package httphandler

import (
	"github.com/google/jsonapi"
	models2 "github.com/mcasperson/OctoPub/go/votes-service/internal/pkg/models"
	"net/http"
	"strconv"
	"time"
)

const (
	headerAccept      = "Accept"
	headerContentType = "Content-Type"
)

type HttpHandler struct{}

func (h *HttpHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {
	if r.Header.Get(headerAccept) != jsonapi.MediaType {
		http.Error(w, "Unsupported Media Type", http.StatusUnsupportedMediaType)
		return
	}

	var methodHandler http.HandlerFunc
	switch r.Method {
	case http.MethodGet:
		if r.FormValue("id") != "" {
			methodHandler = h.showVote
		} else {
			methodHandler = h.listVotes
		}
	default:
		http.Error(w, "Not Found", http.StatusNotFound)
		return
	}

	methodHandler(w, r)
}

func (h *HttpHandler) showVote(w http.ResponseWriter, r *http.Request) {
	id := r.FormValue("id")

	intID, err := strconv.Atoi(id)
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	jsonapiRuntime := jsonapi.NewRuntime().Instrument("votes.show")

	// but, for now
	vote := models2.Vote{
		ID:        intID,
		CreatedAt: time.Time{},
		IPAddress: "",
		Product:   nil,
	}
	w.WriteHeader(http.StatusOK)

	w.Header().Set(headerContentType, jsonapi.MediaType)
	if err := jsonapiRuntime.MarshalPayload(w, vote); err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
	}
}

func (h *HttpHandler) listVotes(w http.ResponseWriter, r *http.Request) {
	jsonapiRuntime := jsonapi.NewRuntime().Instrument("blogs.list")

	product := models2.Product{
		ID: 1,
	}
	votes := []*models2.Vote{{ID: 1, CreatedAt: time.Time{}, IPAddress: "", Product: &product}}

	w.Header().Set("Content-Type", jsonapi.MediaType)
	w.WriteHeader(http.StatusOK)

	if err := jsonapiRuntime.MarshalPayload(w, votes); err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
	}
}
