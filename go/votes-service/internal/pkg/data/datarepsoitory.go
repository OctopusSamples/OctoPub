package data

import "github.com/mcasperson/OctoPub/go/votes-service/internal/pkg/models"

type VotesRepository interface {
	FindOne(id string) (models.Entity, error)
	FindAll(tenant string) ([]models.Entity, error)
}
