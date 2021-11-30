import React, {useReducer, useState} from "react";
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

// define app context
export const AppContext = React.createContext<DynamicConfig>({
    settings: {basename: "", title: "", productEndpoint: "", auditEndpoint: "", editorFormat: "", google: {tag: ""}},
    useDefaultTheme: true,
    apiKey: null,
    setPartition: () => {
    },
    partition: null,
    allBookId: null,
    setAllBookId: () => {
    },
    setApiKey: () => {
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
    const [apiKey, setApiKey] = useState<string | null>(localStorage.getItem("apiKey"));
    const [partition, setPartition] = useState<string | null>(localStorage.getItem("partition") || "main");

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
                setPartition
            }}>
                <ThemeProvider theme={theme}>
                    <HashRouter>
                        <Routes>
                            {routes.map((route: RouteItem) =>
                                route.subRoutes ? (
                                    route.subRoutes.map((item: RouteItem) => (
                                        <Route
                                            key={`${item.key}`}
                                            path={`${item.path}`}
                                            element={<Layout toggleTheme={toggle} useDefaultTheme={useDefaultTheme}>
                                                {item.component()({})}</Layout>}
                                        />
                                    ))
                                ) : (
                                    <Route
                                        key={`${route.key}`}
                                        path={`${route.path}`}
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