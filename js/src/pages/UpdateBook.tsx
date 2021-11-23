import {FC, ReactElement, useContext, useEffect, useState} from "react";
import {CommonProps} from "../model/RouteItem.model";
import {Helmet} from "react-helmet";
import {Button, createStyles, FormLabel, Grid, makeStyles, TextField} from "@material-ui/core";
import {AppContext} from "../App";
import {Product} from "../model/Product";
import {useHistory, useParams} from "react-router-dom";

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

const UpdateBook: FC<CommonProps> = (props: CommonProps): ReactElement => {
    props.setAllBookId(null);

    const {bookId} = useParams<Params>();
    const history = useHistory();
    const context = useContext(AppContext);
    const classes = useStyles();
    const [disabled, setDisabled] = useState<boolean>(true);
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
                dataPartition: ""
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

                if (!props.apiKey) {
                    setError("The API key must be defined in the settings page.");
                } else if (data.properties.dataPartition !== props.partition) {
                    setError("This book belongs to another data partition, and cannot be edited.");
                } else {
                    setDisabled(false);
                }
            });
    }, [setBook, setDisabled, props.apiKey, props.partition, context.settings.productEndpoint, bookId]);

    return (
        <>
            <Helmet>
                <title>
                    {context.settings.title}
                </title>
            </Helmet>
            <Grid container={true}>
                <Grid md={2} sm={12} xs={12}>
                    <FormLabel className={classes.label}>Id</FormLabel>
                </Grid>
                <Grid item md={10} sm={12} xs={12}>
                    <TextField id={"id"} disabled={true} fullWidth={true} variant={"outlined"}
                               value={book.data.id}/>
                </Grid>
                <Grid md={2} sm={12} xs={12}>
                    <FormLabel className={classes.label}>Name</FormLabel>
                </Grid>
                <Grid item md={10} sm={12} xs={12}>
                    <TextField id={"name"} disabled={disabled} fullWidth={true} variant={"outlined"}
                               value={book.data.attributes.name}
                               onChange={v => updateBook(v, "name")}/>
                </Grid>
                <Grid item md={2} sm={12} xs={12}>
                    <FormLabel className={classes.label}>Image</FormLabel>
                </Grid>
                <Grid item md={10} sm={12} xs={12}>
                    <TextField id={"image"} disabled={disabled} fullWidth={true} variant={"outlined"}
                               value={book.data.attributes.image}
                               onChange={v => updateBook(v, "image")}/>
                </Grid>
                <Grid item md={2} sm={12} xs={12}>
                    <FormLabel className={classes.label}>EPUB</FormLabel>
                </Grid>
                <Grid item md={10} sm={12} xs={12}>
                    <TextField id={"epub"} disabled={disabled} fullWidth={true} variant={"outlined"}
                               value={book.data.attributes.epub}
                               onChange={v => updateBook(v, "epub")}/>
                </Grid>
                <Grid item md={2} sm={12} xs={12}>
                    <FormLabel className={classes.label}>PDF</FormLabel>
                </Grid>
                <Grid item md={10} sm={12} xs={12}>
                    <TextField id={"pdf"} disabled={disabled} fullWidth={true} variant={"outlined"}
                               value={book.data.attributes.pdf}
                               onChange={v => updateBook(v, "pdf")}/>
                </Grid>
                <Grid item md={2} sm={12} xs={12}>
                    <FormLabel className={classes.label}>Description</FormLabel>
                </Grid>
                <Grid item md={10} sm={12} xs={12}>
                    <TextField id={"description"} disabled={disabled} multiline={true} rows={10} fullWidth={true}
                               variant={"outlined"}
                               value={book.data.attributes.description}
                               onChange={v => updateBook(v, "description")}/>
                </Grid>
                <Grid item md={2} sm={12} xs={12}>

                </Grid>
                <Grid item md={10} sm={12} xs={12}>
                    <Button variant={"outlined"} disabled={disabled} onClick={_ => saveBook()}>Update Book</Button>
                </Grid>
                <Grid item md={2} sm={12} xs={12}>

                </Grid>
                <Grid item md={10} sm={12} xs={12}>
                    {error && <span>{error}</span>}
                </Grid>
            </Grid>

        </>
    );

    function updateBook(input: React.ChangeEvent<HTMLTextAreaElement | HTMLInputElement>, property: string) {
        setBook({
            data: {
                id: null,
                type: book.data.type,
                attributes: {...book.data.attributes, [property]: input.target.value}
            }
        })
    }

    function saveBook() {
        setDisabled(true);
        fetch(context.settings.productEndpoint + "/" + bookId, {
            method: 'PATCH',
            headers: {
                'Accept': 'application/vnd.api+json; dataPartition=' + props.partition,
                'Content-Type': 'application/vnd.api+json',
                'X-API-Key': props.apiKey || ""
            },
            body: JSON.stringify(book, (key, value) => {
                if (value !== null) return value
            })
        })
            .then((response) => {
                if (response.ok) {
                    return response.json();
                } else {
                    throw new Error('Something went wrong');
                }
            })
            .then(_ => history.push('/index.html'))
            .catch(_ => {
                setDisabled(false);
                setError("An error occurred and the book was not updated.");
            });
    }
}


export default UpdateBook;