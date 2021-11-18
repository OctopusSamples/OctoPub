import {FC, ReactElement, useContext, useEffect, useState} from "react";
import {Helmet} from "react-helmet";
import {createStyles, makeStyles} from "@material-ui/core/styles";
import {Grid, Theme} from "@material-ui/core";

// constants
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
            justifyContent: "center",
            height: "250px"
        }
    })
);

interface Products {
    data: {
        id: number,
        attributes: {
            tenant: string,
            name: string
        }
    }[]
}

const Home: FC<CommonProps> = (props: CommonProps): ReactElement => {

    const classes = useStyles();

    const context = useContext(AppContext);

    const [books, setBooks] = useState<Products | null>(null);

    useEffect(() => {
        fetch(context.settings.productEndpoint, {
            headers: {
                'Accept': 'application/vnd.api+json'
            }
        })
            .then(response => response.json())
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
                          container={true}>
                        <div>{b.attributes.name}</div>
                    </Grid>)}
            </Grid>
        </>
    );
};

export default Home;