// @ts-nocheck

import React from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import App from './App';
import reportWebVitals from './reportWebVitals';
import {loadConfig} from "./dynamicConfig";
import {clearLoginBranch, getLoginBranch} from "./utils/path";
import {setAccessToken} from "./utils/security";

if (handleLogin()) {
    loadConfig().then((config) => {
        setupGoogleAnalytics(config.settings?.google?.tag);

        ReactDOM.render(
            <React.StrictMode>
                <App settings={config.settings}/>
            </React.StrictMode>,
            document.getElementById('root')
        );
    })
}

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();

/**
 * Deal with a redirection from Cognito, handing a second redirect to the original feature branch
 * that initiated the login.
 * @return true if the app should continue loading normally, false if we're redirecting away from this page
 */
function handleLogin() {
    try {
        /*
         In the event of a login error, clear the hash and open the main page.
         Without this, the React router may try to match the hash, which won't work.
         */
        const error = getHashField("error");

        if (error) {
            window.location.href = "/";
            return false;
        }

        const loginBranch = getLoginBranch();
        const accessToken = getHashField("access_token");

        // Before we redirect to cognito, the login branch must be set. If not, ignore redirect.
        if (!loginBranch) {
            if (!accessToken) {
                return true;
            } else {
                // There are values in the hash that will cause issues with routing, so go back to the root.
                window.location.href = "/";
                return false;
            }
        }

        if (accessToken) {
            setAccessToken(accessToken);
            window.location.href = "/" + loginBranch;
            return false;
        }

        return true;
    } finally {
        clearLoginBranch();
    }
}

function getHashField(field: string) {
    // The returned hash from Cognito splits id and access tokens with ampersand
    return window.location.hash
        // drop the leading hash
        .replace("#", "")
        // split on ampersands
        .split("&")
        // The access token starts with this string
        .filter(h => h.startsWith(field))
        // The tokens are name=value
        .map(h => h.split("="))
        // sanity check to make sure we have 2 values
        .filter(h => h.length === 2)
        // get the value
        .map(h => h.pop())
        // there should only be one element
        .pop();
}

function setupGoogleAnalytics(tag: string) {
    if (!tag) return;

    addScript("https://www.googletagmanager.com/gtag/js?id=" + tag, true, false)
    window.dataLayer = window.dataLayer || [];

    function gtag() {
        dataLayer.push(arguments);
    }

    gtag('js', new Date());
    gtag('config', tag);
}

function addScript(src, async, defer) {
    const s = document.createElement('script');
    s.setAttribute('src', src);
    if (async) {
        s.setAttribute('async', 'async')
    }
    if (defer) {
        s.setAttribute('defer', 'defer')
    }
    document.body.appendChild(s);
}