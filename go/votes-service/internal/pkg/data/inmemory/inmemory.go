package inmemory

import (
	"github.com/hashicorp/go-memdb"
	"github.com/mcasperson/OctoPub/go/votes-service/internal/pkg/models"
	"time"
)

type InMemoryDb struct {
	database *memdb.MemDB
}

func New() *InMemoryDb {
	schema := &memdb.DBSchema{
		Tables: map[string]*memdb.TableSchema{
			"vote": &memdb.TableSchema{
				Name: "vote",
				Indexes: map[string]*memdb.IndexSchema{
					"id": &memdb.IndexSchema{
						Name:    "id",
						Unique:  true,
						Indexer: &memdb.StringFieldIndex{Field: "ID"},
					},
					"vote_object": &memdb.IndexSchema{
						Name:    "vote_object",
						Unique:  true,
						Indexer: &memdb.StringFieldIndex{Field: "VoteObject"},
					},
				},
			},
		},
	}

	database, err := memdb.NewMemDB(schema)
	if err != nil {
		panic(err)
	} else {
		inMemoryDb := InMemoryDb{database: database}
		inMemoryDb.initData()
		return &inMemoryDb
	}
}

func (db InMemoryDb) FindOne(id string) (models.Entity, error) {
	txn := db.database.Txn(false)
	defer txn.Abort()
	vote, err := txn.First("vote", "id", id)
	if err != nil {
		return nil, err
	}
	return vote.(models.Entity), nil
}

func (db InMemoryDb) FindAll() ([]models.Entity, error) {
	txn := db.database.Txn(false)
	defer txn.Abort()
	iterator, err := txn.Get("vote", "id")
	if err != nil {
		return nil, err
	}

	votesArray := []models.Entity{}
	for obj := iterator.Next(); obj != nil; obj = iterator.Next() {
		v := obj.(models.Entity)
		votesArray = append(votesArray, v)
	}

	return votesArray, nil
}

func (db InMemoryDb) initData() {
	txn := db.database.Txn(true)
	for _, v := range db.getData() {
		txn.Insert("vote", v)
	}
	txn.Commit()
}

func (db InMemoryDb) getData() []*models.Vote {
	return []*models.Vote{
		{ID: "urn:votes:1", CreatedAt: time.Now(), VoteObject: &models.Urn{ID: "urn:products:1"}},
	}
}
