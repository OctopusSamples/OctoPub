package votes

import (
	"github.com/mcasperson/OctoPub/go/votes-service/pkg/models/product"
	"time"
)

type Vote struct {
	ID        int              `jsonapi:"primary,blogs"`
	CreatedAt time.Time        `jsonapi:"attr,created_at"`
	IPAddress string           `jsonapi:"attr,ip_address"`
	Product   *product.Product `jsonapi:"relation,product"`
}
