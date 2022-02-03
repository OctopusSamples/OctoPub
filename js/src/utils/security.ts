import {getBranch, getLoginBranch, setLoginBranch} from "./path";
import jwt from 'jsonwebtoken';
import jwkToPem, {JWK} from 'jwk-to-pem';

export function setAccessToken(accessToken: string) {
    window.localStorage.setItem(getLoginBranch() + "-accesstoken", accessToken);
}

/**
 * Gets the saved access token.
 * @param jwk sourced from https://cognito-idp.<region>.amazonaws.com/<pool id>/.well-known/jwks.json
 */
export function getAccessToken(jwk: JWK[]) {
    if (!jwk) {
        return "";
    }

    const accessToken = window.localStorage.getItem(getBranch() + "-accesstoken") || "";
    if (accessToken) {
        const anyValidate = jwk.map(j => {
            try {
                const pem = jwkToPem(j);
                jwt.verify(accessToken, pem, {algorithms: ['RS256']});
                return true;
            } catch (err) {
                return false;
            }
        }).find(j => j);
        if (!anyValidate) {
            return "";
        }
    }

    return accessToken;
}

export function clearAccessToken() {
    window.localStorage.setItem(getBranch() + "-accesstoken", "");
}

export function login(cognitoLogin: string) {
    setLoginBranch();
    window.location.href = cognitoLogin;
}

export function logout() {
    clearAccessToken();
}