package inmemory

import (
	"github.com/hashicorp/go-memdb"
	"github.com/mcasperson/OctoPub/go/votes-service/config"
	"github.com/mcasperson/OctoPub/go/votes-service/internal/pkg/models"
	"time"
)

type Db struct {
	database *memdb.MemDB
}

func New() *Db {
	schema := &memdb.DBSchema{
		Tables: map[string]*memdb.TableSchema{
			"vote": {
				Name: "vote",
				Indexes: map[string]*memdb.IndexSchema{
					"id": {
						Name:    "id",
						Unique:  false,
						Indexer: &memdb.StringFieldIndex{Field: "ID"},
					},
					"tenant": {
						Name:    "tenant",
						Unique:  false,
						Indexer: &memdb.StringFieldIndex{Field: "Tenant"},
					},
					"vote_object": {
						Name:    "vote_object",
						Unique:  false,
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
		inMemoryDb := Db{database: database}
		inMemoryDb.initData()
		return &inMemoryDb
	}
}

func (db Db) FindOne(id string, tenant string) (models.Entity, error) {
	txn := db.database.Txn(false)
	defer txn.Abort()
	vote, err := txn.First("vote", "id", id)
	if err != nil {
		return nil, err
	}
	if vote == nil {
		return nil, nil
	}

	// we can only find records for the main or current tenant
	entity := vote.(models.Entity)
	if entity.GetTenant() != config.MainTenant && entity.GetTenant() != tenant {
		return nil, nil
	}

	return entity, nil
}

func (db Db) FindAll(tenant string) ([]models.Entity, error) {
	txn := db.database.Txn(false)
	defer txn.Abort()

	// we read all main votes
	iterator, err := txn.Get("vote", "tenant", config.MainTenant)
	if err != nil {
		return nil, err
	}

	// and we read all votes from our tenant
	iterator2, err2 := txn.Get("vote", "tenant", tenant)
	if err2 != nil {
		return nil, err2
	}

	votesArray := []models.Entity{}
	for _, i := range [2]memdb.ResultIterator{iterator, iterator2} {
		for obj := i.Next(); obj != nil; obj = i.Next() {
			v := obj.(models.Entity)
			votesArray = append(votesArray, v)
		}
	}

	return votesArray, nil
}

func (db Db) initData() {
	txn := db.database.Txn(true)
	for _, v := range db.getData() {
		txn.Insert("vote", v)
	}
	txn.Commit()
}

func (db Db) getData() []*models.Vote {
	return []*models.Vote{
		{
			ID:         "urn:votes:1",
			Tenant:     config.MainTenant,
			CreatedAt:  time.Now(),
			VoteObject: &models.Urn{ID: "urn:products:1"}},
	}
}
