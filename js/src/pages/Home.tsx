import {FC, ReactElement, useContext, useEffect, useState} from "react";
import {Helmet} from "react-helmet";
import {createStyles, makeStyles} from "@material-ui/core/styles";
import {Grid, Theme} from "@material-ui/core";

// constants
import {AppContext} from "../App";
import {CommonProps} from "../model/RouteItem.model";
import {Products} from "../model/Product";
import {useHistory} from "react-router-dom";

// define css-in-js
const useStyles = makeStyles((theme: Theme) =>
    createStyles({
        root: {
            flex: 1,
            display: "flex",
            flexDirection: "row",
            minWidth: "100%",
            minHeight: "100%",
            justifyContent: "center"
        },
        book: {
            justifyContent: "center",
            height: "40%",
            width: "30%",
            paddingBottom: "8px"
        },
        image: {
            objectFit: "contain",
            height: "100%",
            cursor: "pointer"
        }
    })
);

const Home: FC<CommonProps> = (props: CommonProps): ReactElement => {
    props.setAllBookId(null);

    const classes = useStyles();

    const context = useContext(AppContext);

    const history = useHistory();

    const [books, setBooks] = useState<Products | null>(null);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        fetch(context.settings.productEndpoint, {
            headers: {
                'Accept': 'application/vnd.api+json; partition=' + props.partition
            }
        })
            .then((response) => {
                if (response.ok) {
                    return response.json();
                } else {
                    setError("Failed to return the list of books. Please try again later.")
                }
            })
            .then(data => setBooks(data));
    }, [context.settings.productEndpoint, setBooks, setError, props.partition]);

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
                {!books && !error && <div>Loading...</div>}
                {!books && error && <div>{error}</div>}
                {books && books.data.map(b =>
                    <Grid item md={3} sm={6} xs={12}
                          className={classes.book}
                          container={true}
                          onClick={() => {
                              history.push('/book/' + b.id);
                          }}>
                        <img className={classes.image}
                             src={b.attributes.image || "https://via.placeholder.com/300x400"}
                             alt={b.attributes.name || "Unknown"}/>
                    </Grid>)}
            </Grid>
        </>
    );
};

export default Home;