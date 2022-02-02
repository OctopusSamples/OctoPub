const DEFAULT_BRANCH = "main";

/**
 * Get the path from which to load the config.json file.
 */
export function getBaseUrl() {
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

export function getBranch() {
    const baseUrl = getBaseUrl();
    if (baseUrl === "") {
        return DEFAULT_BRANCH;
    }

    return baseUrl.split("/").pop() || "";
}

export function getBranchPath(branch: string) {
    if (branch === DEFAULT_BRANCH) {
        return "/";
    }

    return "/" + branch + "/";
}

export function setLoginBranch() {
    return window.localStorage.setItem("loginbranch", getBranch());
}

export function getLoginBranch() {
    return window.localStorage.getItem("loginbranch");
}

export function clearLoginBranch() {
    return window.localStorage.setItem("loginbranch", "");
}