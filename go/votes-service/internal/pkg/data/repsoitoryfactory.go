package data

import "github.com/mcasperson/OctoPub/go/votes-service/internal/pkg/data/inmemory"

var repo VotesRepository

func GetRepository() VotesRepository {
	if repo != nil {
		return repo
	}
	repo = inmemory.New()
	return repo
}
