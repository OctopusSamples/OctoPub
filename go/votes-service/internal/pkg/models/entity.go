package models

type Entity interface {
	GetID() string
	GetTenant() string
}
