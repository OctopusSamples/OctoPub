// Package models contains the API model representations
package models

import (
	"time"
)

type Vote struct {
	ID         string    `jsonapi:"primary,vote"`
	CreatedAt  time.Time `jsonapi:"attr,created_at"`
	IPAddress  string    `jsonapi:"attr,ip_address"`
	VoteObject *Urn      `jsonapi:"relation,vote_object,omitempty"`
}

func (v *Vote) getID() string {
	return v.ID
}
