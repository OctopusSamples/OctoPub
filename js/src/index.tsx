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
 */
function handleLogin() {
    try {
        const loginBranch = getLoginBranch();

        // Before we redirect to cognito, the login branch must be set. If not, ignore redirect.
        if (!loginBranch) {
            return;
        }

        const accessToken =
            // The returned hash from Cognito splits id and access tokens with ampersand
            window.location.hash.split("&")
                // The access token starts with this string
                .filter(h => h.startsWith("access_token"))
                // The tokens are name=value
                .map(h => h.split("="))
                // sanity check to make sure we have 2 values
                .filter(h => h.length === 2)
                // get the value
                .map(h => h.pop())
                // there should only be one element
                .pop();

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