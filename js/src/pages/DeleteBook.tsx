import {FC, ReactElement, useContext, useEffect, useState} from "react";
import {CommonProps} from "../model/RouteItem.model";
import {Helmet} from "react-helmet";
import {Button, createStyles, FormLabel, Grid, makeStyles, TextField} from "@material-ui/core";
import {AppContext} from "../App";
import {Product} from "../model/Product";
import {useHistory, useParams} from "react-router-dom";
import {useStateWithCallbackLazy} from 'use-state-with-callback';

const useStyles = makeStyles((theme) =>
    createStyles({
        label: {
            color: theme.palette.text.primary
        }
    })
);

interface Params {
    bookId: string
}

const DeleteBook: FC<CommonProps> = (props: CommonProps): ReactElement => {
    props.setDeleteBookId(null);

    const { bookId } = useParams<Params>();
    const history = useHistory();
    const context = useContext(AppContext);
    const classes = useStyles();
    const [disabled, setDisabled] = useStateWithCallbackLazy<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [book, setBook] = useState<Product>({
        data: {
            id: null,
            type: "products",
            attributes: {
                name: "",
                description: "",
                image: "",
                epub: "",
                pdf: "",
                tenant: ""
            }
        }
    });

    useEffect(() => {
        fetch(context.settings.productEndpoint + "/" + bookId, {
            headers: {
                'Accept': 'application/vnd.api+json'
            }
        })
            .then(response => response.json())
            .then(data => {
                setBook(data);
            });
    }, [setBook, context.settings.productEndpoint,bookId]);

    useEffect(() => {
        if (props.apiKey) {
            setDisabled(false, () => {});
        } else {
            setError("The API key must be defined in the settings page.");
        }
    }, [setDisabled, setError, props.apiKey]);

    return (
        <>
            <Helmet>
                <title>
                    {context.settings.title}
                </title>
            </Helmet>
            <Grid container={true}>
                <Grid md={2} sm={12}>
                    <FormLabel className={classes.label}>Name</FormLabel>
                </Grid>
                <Grid item md={10} sm={12}>
                    <TextField id={"name"} disabled={true} fullWidth={true} variant={"outlined"}
                               value={book.data.attributes.name}/>
                </Grid>
                <Grid item md={2} sm={12}>
                    <FormLabel className={classes.label}>Image</FormLabel>
                </Grid>
                <Grid item md={10} sm={12}>
                    <TextField id={"image"} disabled={true} fullWidth={true} variant={"outlined"}
                               value={book.data.attributes.image}/>
                </Grid>
                <Grid item md={2} sm={12}>
                    <FormLabel className={classes.label}>EPUB</FormLabel>
                </Grid>
                <Grid item md={10} sm={12}>
                    <TextField id={"epub"} disabled={true} fullWidth={true} variant={"outlined"}
                               value={book.data.attributes.epub}/>
                </Grid>
                <Grid item md={2} sm={12}>
                    <FormLabel className={classes.label}>PDF</FormLabel>
                </Grid>
                <Grid item md={10} sm={12}>
                    <TextField id={"pdf"} disabled={true} fullWidth={true} variant={"outlined"}
                               value={book.data.attributes.pdf}/>
                </Grid>
                <Grid item md={2} sm={12}>
                    <FormLabel className={classes.label}>Description</FormLabel>
                </Grid>
                <Grid item md={10} sm={12}>
                    <TextField id={"description"} disabled={true} multiline={true} rows={10} fullWidth={true}
                               variant={"outlined"}
                               value={book.data.attributes.description}/>
                </Grid>
                <Grid item md={2} sm={12}>

                </Grid>
                <Grid item md={10} sm={12}>
                    <Button variant={"outlined"} disabled={disabled} onClick={_ => deleteBook()}>Delete Book</Button>
                </Grid>
                <Grid item md={2} sm={12}>

                </Grid>
                <Grid item md={10} sm={12}>
                    {error && <span>{error}</span>}
                </Grid>
            </Grid>

        </>
    );

    function deleteBook() {
        setDisabled(true, () =>
            fetch(context.settings.productEndpoint + "/" + bookId, {
                method: 'DELETE',
                headers: {
                    'Accept': 'application/vnd.api+json',
                    'Content-Type': 'application/vnd.api+json',
                    'X-API-Key': props.apiKey || ""
                },
                body: JSON.stringify(book, (key, value) => {
                    if (value !== null) return value
                })
            })
                .then((response) => {
                    if (!response.ok) {
                        throw new Error('Something went wrong')
                    }
                })
                .then(_ => history.push('/index.html'))
                .catch(_ => {
                    setDisabled(false, () => {});
                    setError("An error occurred while deleting the book.");
                })
        );
    }
}


export default DeleteBook;