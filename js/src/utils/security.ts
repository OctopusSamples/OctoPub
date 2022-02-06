import {getBranch, getLoginBranch, setLoginBranch} from "./path";
import jwt from 'jsonwebtoken';
import jwkToPem, {JWK} from 'jwk-to-pem';

export function setAccessToken(accessToken: string) {
    window.localStorage.setItem(getLoginBranch() + "-accesstoken", accessToken);
}

export function setIdToken(idToken: string) {
    window.localStorage.setItem(getLoginBranch() + "-idtoken", idToken);
}

export function setTokenExpiry(expiry: string) {
    const expiryInt = parseInt(expiry);
    if (isNaN(expiryInt)) {
        // bad expiry, so we expire in the past
        window.localStorage.setItem(getLoginBranch() + "-tokenexpiry", new Date().getSeconds().toFixed(0));
    } else {
        const now = new Date();
        window.localStorage.setItem(getLoginBranch() + "-tokenexpiry", now.setSeconds(now.getSeconds() + expiryInt).toFixed(0));
    }
}

/**
 * Gets the saved access token.
 * @param jwk sourced from https://cognito-idp.<region>.amazonaws.com/<pool id>/.well-known/jwks.json
 */
export function getIdToken(jwk: JWK[]) {
    if (!jwk) {
        return "";
    }

    if (isTokenExpired()) {
        return "";
    }

    const idToken = window.localStorage.getItem(getBranch() + "-idtoken") || "";
    if (idToken) {
        const anyValidate = jwk.map(j => {
            try {
                const pem = jwkToPem(j);
                jwt.verify(idToken, pem, {algorithms: ['RS256']});
                return true;
            } catch (err) {
                return false;
            }
        }).find(j => j);
        if (!anyValidate) {
            return "";
        }
    }

    return idToken;
}

/**
 * Gets the saved access token.
 */
export function getAccessToken() {
    if (isTokenExpired()) {
        return "";
    }

    return window.localStorage.getItem(getBranch() + "-accesstoken") || "";
}

export function clearTokens() {
    window.localStorage.setItem(getBranch() + "-accesstoken", "");
}

export function login(cognitoLogin: string) {
    setLoginBranch();
    window.location.href = cognitoLogin;
}

export function logout() {
    clearTokens();
}

function isTokenExpired() {
    // Check the token expiry
    const expiry = parseInt(window.localStorage.getItem(getBranch() + "-tokenexpiry") || "");
    if (isNaN(expiry) || new Date(expiry * 1000) < new Date()) {
        return true;
    }

    return false;
}