package models

type Entity interface {
	getID() string
	getTenant() string
}
