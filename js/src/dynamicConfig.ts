import {DynamicConfig} from "./config/dynamicConfig";

/**
 * We need for this application to work under a variety of subpaths.
 * This function assumes the path that was used to access the index file
 * represents the base path that other pages will be found from.
 */
export async function loadConfig(): Promise<DynamicConfig> {
    const baseUrl = getBaseUrl();
    const defaultResponse = {settings: {basename: baseUrl}};

    const config = await fetch(baseUrl + "/config.json")
        .then(response => response.ok ? response.json() : defaultResponse)
        .catch(error => defaultResponse);
    // Set some default values if the config file was not present or not configured
    config.settings.title = config.settings.title || "OctoPub";
    config.settings.productEndpoint = config.settings.productEndpoint || "http://localhost:8083/api/products";
    config.settings.auditEndpoint = config.settings.auditEndpoint || "http://localhost:9080/api/audits";
    config.settings.healthEndpoint = config.settings.healthEndpoint || "http://localhost:6080/health";
    config.settings.requireApiKey = config.settings.requireApiKey || "false";
    return config;
}

/**
 * Get the path from which to load the config.json file.
 */
function getBaseUrl() {
    try {
        const url = window.location.pathname;
        if (url.endsWith(".html") || url.endsWith(".htm")) {
            return url.substr(0, url.lastIndexOf('/'));
        } else if (url.endsWith("/")) {
            return url.substring(0, url.length - 1);
        }

        return url;
    } catch {
       return "";
    }
}