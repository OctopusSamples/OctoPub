import {getBranch, getLoginBranch} from "./path";

export function setAccessToken(accessToken: string) {
    window.localStorage.setItem(getLoginBranch() + "-accesstoken", accessToken);
}

export function getAccessToken() {
    return window.localStorage.getItem(getLoginBranch() + "-accesstoken") || "";
}

export function clearAccessToken() {
    window.localStorage.setItem(getBranch() + "-accesstoken", "");
}