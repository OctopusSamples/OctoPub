// Package models contains the API model representations
package models

import (
	"time"
)

type Vote struct {
	ID        int       `jsonapi:"primary,blogs"`
	CreatedAt time.Time `jsonapi:"attr,created_at"`
	IPAddress string    `jsonapi:"attr,ip_address"`
	Product   *Product  `jsonapi:"relation,product,omitempty"`
}
