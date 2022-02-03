import {GET_RETRIES} from "./constants";
import {RedirectRule} from "../pages/Branching";

export function isBranchingEnabled() {
    return (localStorage.getItem("branchingEnabled") || "").toLowerCase() !== "false";
}

function getBranchingRules() {
    if (isBranchingEnabled()) {
        const rules: RedirectRule[] = JSON.parse(localStorage.getItem("branching") || "[]")
        return rules
            .filter(r => r.path.trim() && r.destination.trim())
            .map(r => "version[" + r.path + "]=" + r.destination).join(";")
    }

    return "";
}

function responseIsServerError(status: number) {
    return status >= 500 && status <= 599;
}

export function getJson<T>(url: string, accessToken?: string | null, retryCount?: number): Promise<T> {
    return fetch(url, {
        method: 'GET',
        headers: {
            'Accept': 'application/json',
            'Authorization': accessToken ? "Bearer: " + accessToken : ""
        }
    })
        .then(response => {
            if (!responseIsServerError(response.status)) {
                return response.json();
            }
            if ((retryCount || 0) <= GET_RETRIES) {
                /*
                 Some lambdas are slow, and initial requests timeout with a 504 response.
                 We automatically retry these requests.
                 */
                return getJson<T>(url, accessToken, (retryCount || 0) + 1);
            }
            return Promise.reject(response);
        });
}

export function getJsonApi<T>(url: string, partition: string | null, accessToken?: string | null, retryCount?: number): Promise<T> {
    return fetch(url, {
        method: 'GET',
        headers: {
            'Accept': 'application/vnd.api+json, application/vnd.api+json; dataPartition=' + partition + '; ' + getBranchingRules(),
            'Content-Type': 'application/vnd.api+json',
            'Authorization': accessToken ? "Bearer: " + accessToken : ""
        }
    })
        .then(response => {
            if (!responseIsServerError(response.status)) {
                return response.json();
            }
            if ((retryCount || 0) <= GET_RETRIES) {
                /*
                 Some lambdas are slow, and initial requests timeout with a 504 response.
                 We automatically retry these requests.
                 */
                return getJsonApi<T>(url, partition, accessToken, (retryCount || 0) + 1);
            }
            return Promise.reject(response);
        });
}

export function patchJsonApi<T>(resource: string, url: string, partition: string | null, accessToken?: string | null, retryCount?: number): Promise<T> {
    return fetch(url, {
        method: 'PATCH',
        headers: {
            'Accept': 'application/vnd.api+json, application/vnd.api+json; dataPartition=' + partition + '; ' + getBranchingRules(),
            'Content-Type': 'application/vnd.api+json',
            'Authorization': accessToken ? "Bearer: " + accessToken : ""
        },
        body: resource
    })
        .then(response => {
            if (!responseIsServerError(response.status)) {
                return response.json();
            }
            if ((retryCount || 0) <= GET_RETRIES) {
                /*
                 Some lambdas are slow, and initial requests timeout with a 504 response.
                 We automatically retry these requests.
                 */
                return patchJsonApi<T>(resource, url, partition, accessToken, (retryCount || 0) + 1);
            }
            return Promise.reject(response);
        });
}

export function postJsonApi<T>(resource: string, url: string, partition: string | null, accessToken?: string | null): Promise<T> {
    return fetch(url, {
        method: 'POST',
        headers: {
            'Accept': 'application/vnd.api+json, application/vnd.api+json; dataPartition=' + partition + '; ' + getBranchingRules(),
            'Content-Type': 'application/vnd.api+json',
            'Authorization': accessToken ? "Bearer: " + accessToken : ""
        },
        body: resource
    })
        .then(response => {
            if (response.ok) {
                return response.json();
            }
            return Promise.reject(response);
        });
}

export function deleteJsonApi(url: string, partition: string | null, accessToken?: string | null, retryCount?: number): Promise<Response> {
    return fetch(url, {
        method: 'DELETE',
        headers: {
            'Accept': 'application/vnd.api+json, application/vnd.api+json; dataPartition=' + partition + '; ' + getBranchingRules(),
            'Content-Type': 'application/vnd.api+json',
            'Authorization': accessToken ? "Bearer: " + accessToken : ""
        }
    })
        .then(response => {
            if (!responseIsServerError(response.status)) {
                return Promise.resolve(response);
            }
            if ((retryCount || 0) <= GET_RETRIES) {
                /*
                 Some lambdas are slow, and initial requests timeout with a 504 response.
                 We automatically retry these requests.
                 */
                return deleteJsonApi(url, partition, accessToken, (retryCount || 0) + 1);
            }
            return Promise.reject(response);
        });
}