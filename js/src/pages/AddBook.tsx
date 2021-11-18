import {FC, ReactElement, useContext, useState} from "react";
import {CommonProps} from "../model/RouteItem.model";
import {Helmet} from "react-helmet";
import {Button, createStyles, FormLabel, Grid, makeStyles, TextField} from "@material-ui/core";
import {AppContext} from "../App";
import {Product} from "../model/Product";
import {useHistory} from "react-router-dom";
import { useStateWithCallbackLazy } from 'use-state-with-callback';

const useStyles = makeStyles(() =>
    createStyles({
        label: {}
    })
);

const AddBook: FC<CommonProps> = (props: CommonProps): ReactElement => {
    const history = useHistory();
    const context = useContext(AppContext);
    const classes = useStyles();
    const [disabled, setDisabled] = useStateWithCallbackLazy<boolean>(false);
    const [error, setError] = useState<string | null>(null);
    const [book, setBook] = useState<Product>({
        data: {
            id: null,
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

    return (
        <>
            <Helmet>
                <title>
                    {context.settings.title}
                </title>
            </Helmet>
            <Grid container={true}>
                <Grid className={classes.label} md={2} sm={12}>
                    <FormLabel>Name</FormLabel>
                </Grid>
                <Grid item md={10} sm={12}>
                    <TextField id={"name"} fullWidth={true} variant={"outlined"} value={book.data.attributes.name}
                               onChange={v => updateBook(v, "name")}/>
                </Grid>
                <Grid item md={2} sm={12}>
                    <FormLabel>Image</FormLabel>
                </Grid>
                <Grid item md={10} sm={12}>
                    <TextField id={"image"} fullWidth={true} variant={"outlined"} value={book.data.attributes.image}
                               onChange={v => updateBook(v, "image")}/>
                </Grid>
                <Grid item md={2} sm={12}>
                    <FormLabel>EPUB</FormLabel>
                </Grid>
                <Grid item md={10} sm={12}>
                    <TextField id={"epub"} fullWidth={true} variant={"outlined"} value={book.data.attributes.epub}
                               onChange={v => updateBook(v, "epub")}/>
                </Grid>
                <Grid item md={2} sm={12}>
                    <FormLabel>PDF</FormLabel>
                </Grid>
                <Grid item md={10} sm={12}>
                    <TextField id={"pdf"} fullWidth={true} variant={"outlined"} value={book.data.attributes.pdf}
                               onChange={v => updateBook(v, "pdf")}/>
                </Grid>
                <Grid item md={2} sm={12}>
                    <FormLabel>Description</FormLabel>
                </Grid>
                <Grid item md={10} sm={12}>
                    <TextField id={"description"} multiline={true} rows={10} fullWidth={true} variant={"outlined"}
                               value={book.data.attributes.description}
                               onChange={v => updateBook(v, "description")}/>
                </Grid>
                <Grid item md={2} sm={12}>

                </Grid>
                <Grid item md={10} sm={12}>
                    <Button variant={"outlined"} disabled={disabled} onClick={_ => saveBook()}>Create New Book</Button>
                </Grid>
                <Grid item md={2} sm={12}>

                </Grid>
                <Grid item md={10} sm={12}>
                    {error && <span>{error}</span>}
                </Grid>
            </Grid>

        </>
    );

    function updateBook(input: React.ChangeEvent<HTMLTextAreaElement | HTMLInputElement>, property: string) {
        setBook({data: {id: null, attributes: {...book.data.attributes, [property]: input.target.value}}})
    }

    function saveBook() {
        setDisabled(true, () =>
            fetch(context.settings.productEndpoint, {
                method: 'POST',
                headers: {
                    'Accept': 'application/vnd.api+json'
                },
                body: JSON.stringify(book)
            })
                .then(response => response.json())
                .then(_ => history.push('/index.html'))
                .catch(_ => {
                    setDisabled(false, () => {});
                    setError("An error occurred and the book was not saved.");
                })
        );
    }
}


export default AddBook;