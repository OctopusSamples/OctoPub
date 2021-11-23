export function getJsonApi<T>(url: string, partition: string | null, apiKey?: string | null): Promise<T> {
    return fetch(url, {
        method: 'GET',
        headers: {
            'Accept': 'application/vnd.api+json; dataPartition=' + partition,
            'Content-Type': 'application/vnd.api+json',
            'X-API-Key': apiKey || ""
        }
    })
        .then(response => {
            if (response.ok) {
                return response.json();
            }
            return Promise.reject(response);
        });
}

export function patchJsonApi<T>(resource: string, url: string, partition: string | null, apiKey?: string | null): Promise<T> {
    return fetch(url, {
        method: 'PATCH',
        headers: {
            'Accept': 'application/vnd.api+json; dataPartition=' + partition,
            'Content-Type': 'application/vnd.api+json',
            'X-API-Key': apiKey || ""
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

export function postJsonApi<T>(resource: string, url: string, partition: string | null, apiKey?: string | null): Promise<T> {
    return fetch(url, {
        method: 'POST',
        headers: {
            'Accept': 'application/vnd.api+json; dataPartition=' + partition,
            'Content-Type': 'application/vnd.api+json',
            'X-API-Key': apiKey || ""
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

export function deleteJsonApi(url: string, partition: string | null, apiKey?: string | null): Promise<Response> {
    return fetch(url, {
        method: 'DELETE',
        headers: {
            'Accept': 'application/vnd.api+json; dataPartition=' + partition,
            'Content-Type': 'application/vnd.api+json',
            'X-API-Key': apiKey || ""
        }
    })
        .then(response => {
            if (!response.ok) {
                return Promise.reject(response);
            }
            return response;
        });
}