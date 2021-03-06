package httphandler

import (
	"github.com/mcasperson/OctoPub/go/votes-service/config"
	"net/http"
	"strings"
)

func GetTenant(r *http.Request) string {
	for _, h := range r.Header.Values(headerAccept) {
		split := strings.Split(h, ";")
		if len(split) > 1 {
			for _, s := range split {
				trimmed := strings.TrimSpace(s)
				// we see if this application is working on a feature branch, or if a general tenant has been defined
				if strings.HasPrefix(trimmed, "tenant=") {
					tenant := strings.Split(trimmed, "=")
					if len(tenant) == 2 {
						return tenant[1]
					}
				}
			}
		}
	}

	return config.MainTenant
}
