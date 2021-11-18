import {FC, ReactElement, useContext, useState} from "react";
import {Helmet} from "react-helmet";
import {createStyles, makeStyles} from "@material-ui/core/styles";
import {Button, FormLabel, Grid, Link, TextField, Theme} from "@material-ui/core";

// constants
import {useHistory} from "react-router-dom";
import {AppContext} from "../App";
import {CommonProps} from "../model/RouteItem.model";

// define css-in-js
const useStyles = makeStyles((theme: Theme) =>
    createStyles({
        root: {
            flex: 1,
            display: "flex",
            flexDirection: "row",
            minWidth: "100%",
            minHeight: "100%",
            height: "80vh",
            justifyContent: "center"
        },
        book: {
            justifyContent: "center"
        }
    })
);

const Home: FC<CommonProps> = (props: CommonProps): ReactElement => {

    const history = useHistory();

    const classes = useStyles();

    const context = useContext(AppContext);

    return (
        <>
            <Helmet>
                <title>
                    {context.settings.title}
                </title>
            </Helmet>
            <Grid
                container={true}
                className={classes.root}
                xs={12}
            >
                <Grid item xs={2}
                      className={classes.book}
                      container={true}>
                    <div>Book 1</div>
                </Grid>
                <Grid item xs={2}
                      className={classes.book}
                      container={true}>
                    <div>Book 2</div>
                </Grid>
                <Grid item xs={2}
                      className={classes.book}
                      container={true}>
                    <div>Book 3</div>
                </Grid>
            </Grid>
        </>
    );
};

export default Home;