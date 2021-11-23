import {FC, ReactElement, useContext, useEffect, useState} from "react";
import {CommonProps} from "../model/RouteItem.model";
import {Helmet} from "react-helmet";
import {Button, FormLabel, Grid, TextField} from "@material-ui/core";
import {AppContext} from "../App";
import {Product} from "../model/Product";
import {useHistory, useParams} from "react-router-dom";
import {styles} from "../styles";

interface Params {
    bookId: string
}

const DeleteBook: FC<CommonProps> = (props: CommonProps): ReactElement => {
    props.setAllBookId(null);

    const {bookId} = useParams<Params>();
    const history = useHistory();
    const context = useContext(AppContext);
    const classes = styles();
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
                'Accept': 'application/vnd.api+json; dataPartition=' + props.partition,
            }
        })
            .then(response => {
                if (response.ok) {
                    return response.json();
                }
                return Promise.reject(response);
            })
            .then(data => {
                setBook(data);

                if (!props.apiKey) {
                    setError("The API key must be defined in the settings page.");
                } else if (data?.data?.attributes?.dataPartition !== props.partition) {
                    setError("This book belongs to the "
                        + data?.data?.attributes?.dataPartition
                        + " data partition, and cannot be deleted.");
                } else {
                    setDisabled(false);
                }
            })
            .catch(() => setError("There was an error retrieving the resource."));
    }, [setBook, setDisabled, props.apiKey, props.partition, context.settings.productEndpoint, bookId]);

    return (
        <>
            <Helmet>
                <title>
                    {context.settings.title}
                </title>
            </Helmet>
            <Grid container={true}>
                <Grid container={true} className={classes.cell} md={2} sm={12} xs={12}>
                    <FormLabel className={classes.label}>Name</FormLabel>
                </Grid>
                <Grid container={true} className={classes.cell} item md={10} sm={12} xs={12}>
                    <TextField id={"name"} disabled={true} fullWidth={true} variant={"outlined"}
                               value={book.data.attributes.name}/>
                </Grid>
                <Grid container={true} className={classes.cell} item md={2} sm={12} xs={12}>
                    <FormLabel className={classes.label}>Image</FormLabel>
                </Grid>
                <Grid container={true} className={classes.cell} item md={10} sm={12} xs={12}>
                    <TextField id={"image"} disabled={true} fullWidth={true} variant={"outlined"}
                               value={book.data.attributes.image}/>
                </Grid>
                <Grid container={true} className={classes.cell} item md={2} sm={12} xs={12}>
                    <FormLabel className={classes.label}>EPUB</FormLabel>
                </Grid>
                <Grid container={true} className={classes.cell} item md={10} sm={12} xs={12}>
                    <TextField id={"epub"} disabled={true} fullWidth={true} variant={"outlined"}
                               value={book.data.attributes.epub}/>
                </Grid>
                <Grid container={true} className={classes.cell} item md={2} sm={12} xs={12}>
                    <FormLabel className={classes.label}>PDF</FormLabel>
                </Grid>
                <Grid container={true} className={classes.cell} item md={10} sm={12} xs={12}>
                    <TextField id={"pdf"} disabled={true} fullWidth={true} variant={"outlined"}
                               value={book.data.attributes.pdf}/>
                </Grid>
                <Grid container={true} className={classes.cell} item md={2} sm={12} xs={12}>
                    <FormLabel className={classes.label}>Description</FormLabel>
                </Grid>
                <Grid container={true} className={classes.cell} item md={10} sm={12} xs={12}>
                    <TextField id={"description"} disabled={true} multiline={true} rows={10} fullWidth={true}
                               variant={"outlined"}
                               value={book.data.attributes.description}/>
                </Grid>
                <Grid container={true} className={classes.cell} item md={2} sm={12} xs={12}>

                </Grid>
                <Grid container={true} className={classes.cell} item md={10} sm={12} xs={12}>
                    <Button variant={"outlined"} disabled={disabled} onClick={_ => deleteBook()}>Delete Book</Button>
                </Grid>
                <Grid container={true} className={classes.cell} item md={2} sm={12} xs={12}>

                </Grid>
                <Grid container={true} className={classes.cell} item md={10} sm={12} xs={12}>
                    {error && <span>{error}</span>}
                </Grid>
            </Grid>

        </>
    );

    function deleteBook() {
        setDisabled(true);
        fetch(context.settings.productEndpoint + "/" + bookId, {
            method: 'DELETE',
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
                if (!response.ok) {
                    throw new Error('Something went wrong')
                }
            })
            .then(_ => history.push('/index.html'))
            .catch(_ => {
                setDisabled(false);
                setError("An error occurred while deleting the book.");
            });
    }
}


export default DeleteBook;