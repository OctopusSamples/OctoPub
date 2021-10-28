package models

type Urn struct {
	ID     string `jsonapi:"primary,urn"`
	Tenant string `jsonapi:"attr,tenant"`
}

func (v Urn) getID() string {
	return v.ID
}

func (v Urn) getTenant() string {
	return v.Tenant
}
