package utils

import "os"

// GetEnv returns the environment variable, or if it is not defined returns the fallback
func GetEnv(key, fallback string) string {
	if value, ok := os.LookupEnv(key); ok {
		return value
	}
	return fallback
}
