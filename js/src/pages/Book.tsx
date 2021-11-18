import {FC, ReactElement, useContext, useEffect, useState} from "react";
import {CommonProps} from "../model/RouteItem.model";
import {Helmet} from "react-helmet";
import {Grid, Theme} from "@material-ui/core";
import {AppContext} from "../App";
import {Product} from "../model/Product";
import {useParams} from "react-router-dom";
import {createStyles, makeStyles} from "@material-ui/core/styles";

interface Params {
    bookId: string
}

const useStyles = makeStyles((theme: Theme) =>
    createStyles({
        image: {
            objectFit: "contain",
            padding: "64px",
            width: "100%"
        }
    })
);

const Book: FC<CommonProps> = (props: CommonProps): ReactElement => {

    const classes = useStyles();

    const context = useContext(AppContext);

    const { bookId } = useParams<Params>();

    const [book, setBook] = useState<Product | null>(null);

    useEffect(() => {
        fetch(context.settings.productEndpoint + "/" + bookId, {
            headers: {
                'Accept': 'application/vnd.api+json'
            }
        })
            .then(response => response.json())
            .then(data => setBook(data));
    }, [bookId, setBook]);

    return (
        <>
            <Helmet>
                <title>
                    {context.settings.title}
                </title>
            </Helmet>
            {!book && <div>Loading...</div>}
            {book && <Grid container={true}>
                <Grid item md={4} sm={12}>
                    <img className={classes.image} src={book.data.attributes.image} alt={book.data.attributes.name}/>
                </Grid>
                <Grid item md={8} sm={12}>
                    <h1>{book.data.attributes.name}</h1>
                    <p>{book.data.attributes.description}</p>
                    <h2>Downloads</h2>
                    <ul>
                        {book.data.attributes.pdf && <li><a href={book.data.attributes.pdf}>PDF</a></li>}
                        {book.data.attributes.epub && <li><a href={book.data.attributes.epub}>EPUB</a></li>}
                    </ul>
                </Grid>
            </Grid>}

        </>
    );
}

export default Book;