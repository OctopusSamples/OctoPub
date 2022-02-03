import React, {useEffect, useReducer, useState} from "react";
import {createTheme, responsiveFontSizes, Theme, ThemeProvider,} from "@material-ui/core/styles";
import {HashRouter, Route, Routes} from "react-router-dom";
import {Helmet} from "react-helmet";
// app routes
// components
import Layout from "./components/Layout";
import jwt_decode from 'jwt-decode';

// theme
import {darkTheme, lightTheme} from "./theme/appTheme";

// interfaces
import RouteItem from "./model/RouteItem.model";
import {DynamicConfig} from "./config/dynamicConfig";
import {routes} from "./config";
import {DEFAULT_BRANCH, getBranch} from "./utils/path";
import {getAccessToken} from "./utils/security";
import Login from "./pages/Login";

// define app context
export const AppContext = React.createContext<DynamicConfig>({
    settings: {
        title: "",
        productEndpoint: "",
        auditEndpoint: "",
        healthEndpoint: "",
        google: {tag: "", oauthClientId: ""},
        aws: {cognitoLogin: "", cognitoDeveloperGroup: "", jwk: {keys: []}}
    },
    useDefaultTheme: true,
    setPartition: () => {
    },
    partition: null,
    allBookId: null,
    setAllBookId: () => {
    }
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
    const [partition, setPartition] = useState<string | null>(localStorage.getItem("partition") || "main");
    const [requireLogin, setRequireLogin] = useState<boolean>(false);

    const keys = config.settings.aws?.jwk?.keys;
    const developerGroup = config.settings.aws?.cognitoDeveloperGroup;

    useEffect(() => {
        const branch = getBranch();
        if (branch !== DEFAULT_BRANCH) {
            const accessToken = getAccessToken(keys);
            if (accessToken) {
                const decoded: any = jwt_decode(accessToken);
                setRequireLogin(decoded["cognito:groups"].indexOf(developerGroup) == -1);
            } else {
                setRequireLogin(true)
            }
        }

        setRequireLogin(false);
    }, [keys, developerGroup]);


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
                partition,
                setPartition
            }}>
                <ThemeProvider theme={theme}>
                    {requireLogin && <Login/>}
                    {!requireLogin &&
                        <HashRouter>
                            <Routes>
                                {routes.map((route: RouteItem) =>
                                    route.subRoutes ? (
                                        route.subRoutes.map((item: RouteItem) => (
                                            <Route
                                                key={item.key}
                                                path={item.path}
                                                element={<Layout toggleTheme={toggle} useDefaultTheme={useDefaultTheme}>
                                                    {item.component()({})}</Layout>}
                                            />
                                        ))
                                    ) : (
                                        <Route
                                            key={route.key}
                                            path={route.path}
                                            element={<Layout toggleTheme={toggle}
                                                             useDefaultTheme={useDefaultTheme}>{route.component()({})}</Layout>}
                                        />
                                    )
                                )
                                }
                            </Routes>
                        </HashRouter>
                    }
                </ThemeProvider>
            </AppContext.Provider>
        </>
    );
}

export default App;