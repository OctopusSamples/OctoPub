import React, {useEffect, useReducer, useState} from "react";
import {createTheme, responsiveFontSizes, Theme, ThemeProvider,} from "@material-ui/core/styles";
import {HashRouter, Route, Routes} from "react-router-dom";
import {Helmet} from "react-helmet";
// app routes
// components
import Layout from "./components/Layout";

// theme
import {darkTheme, lightTheme} from "./theme/appTheme";

// interfaces
import RouteItem from "./model/RouteItem.model";
import {DynamicConfig} from "./config/dynamicConfig";
import {routes} from "./config";

// The google api library
declare var gapi: any;

// define app context
export const AppContext = React.createContext<DynamicConfig>({
    settings: {basename: "", title: "", productEndpoint: "", auditEndpoint: "", healthEndpoint: "", requireApiKey: "", google: {tag: "", oauthClientId: ""}},
    useDefaultTheme: true,
    apiKey: null,
    setPartition: () => {},
    partition: null,
    allBookId: null,
    setAllBookId: () => {},
    setApiKey: () => {},
    googleAuth: null
});

function App(config: DynamicConfig) {

    const [useDefaultTheme, toggle] = useReducer(
        (theme) => {
            localStorage.setItem('defaultTheme', String(!theme));
            return !theme;
        },
        localStorage.getItem('defaultTheme') !== "false");

    // define custom theme
    let theme: Theme = createTheme(useDefaultTheme ? lightTheme : darkTheme);
    theme = responsiveFontSizes(theme);

    const [allBookId, setAllBookId] = useState<string | null>(null);
    const [apiKey, setApiKey] = useState<string | null>(localStorage.getItem("apiKey"));
    const [partition, setPartition] = useState<string | null>(localStorage.getItem("partition") || "main");
    const [googleAuth, setGoogleAuth] = useState<any | null>(null);

    useEffect(() => {
        if (config.settings.google.oauthClientId) {
            gapi.load('auth2', function () {
                gapi.auth2.init({
                    client_id: config.settings.google.oauthClientId + '.apps.googleusercontent.com'
                }).then(function () {
                    setGoogleAuth(gapi.auth2.getAuthInstance());
                });
            });
        }
    });

    return (
        <>
            <Helmet>
                <title>{config.settings.title}</title>
            </Helmet>
            <AppContext.Provider value={{
                ...config,
                useDefaultTheme,
                allBookId,
                setAllBookId,
                apiKey,
                setApiKey,
                partition,
                setPartition,
                googleAuth
            }}>
                <ThemeProvider theme={theme}>
                    <HashRouter basename={config.settings.basename}>
                        <Routes>
                            {routes.map((route: RouteItem) =>
                                route.subRoutes ? (
                                    route.subRoutes.map((item: RouteItem) => (
                                        <Route
                                            key={`${item.key}`}
                                            path={config.settings.basename + item.path}
                                            element={<Layout toggleTheme={toggle} useDefaultTheme={useDefaultTheme}>
                                                {item.component()({})}</Layout>}
                                        />
                                    ))
                                ) : (
                                    <Route
                                        key={`${route.key}`}
                                        path={config.settings.basename + route.path}
                                        element={<Layout toggleTheme={toggle}
                                                         useDefaultTheme={useDefaultTheme}>{route.component()({})}</Layout>}
                                    />
                                )
                            )
                            }
                        </Routes>
                    </HashRouter>
                </ThemeProvider>
            </AppContext.Provider>
        </>
    );
}

export default App;