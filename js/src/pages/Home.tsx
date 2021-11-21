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
            height: "80vh",
            justifyContent: "center"
        },
        book: {
            justifyContent: "center",
            height: "40%",
            width: "30%"
        },
        image: {
            objectFit: "contain",
            height: "100%",
            width: "100%"
        }
    })
);

const Home: FC<CommonProps> = (props: CommonProps): ReactElement => {
    props.setDeleteBookId(null);

    const classes = useStyles();

    const context = useContext(AppContext);

    const history = useHistory();

    const [books, setBooks] = useState<Products | null>(null);

    useEffect(() => {
        fetch(context.settings.productEndpoint, {
            headers: {
                'Accept': 'application/vnd.api+json'
            }
        })
            .then((response) => {
                if (response.ok) {
                    return response.json();
                } else {
                    throw new Error('Something went wrong');
                }
            })
            .then(data => setBooks(data));
    }, [context.settings.productEndpoint, setBooks]);

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
                {!books && <div>Loading...</div>}
                {books && books.data.map(b =>
                    <Grid item xs={4}
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